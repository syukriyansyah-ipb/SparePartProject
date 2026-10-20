using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoPartsApi.Data;

namespace AutoPartsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AnalyticsController(AppDbContext db)
    {
        _db = db;
    }

    // 1. Ambil data KPI (Total Item, Nilai Aset, Stok Kritis)
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var activeParts = await _db.Parts.Where(p => p.IsActive).ToListAsync();

        var totalItems = activeParts.Count;
        var totalValue = activeParts.Sum(p => p.Price * p.Stock);
        var lowStockCount = activeParts.Count(p => p.Stock <= p.MinimumStock);

        return Ok(new
        {
            TotalItems = totalItems,
            TotalValue = totalValue,
            LowStockCount = lowStockCount
        });
    }

    // 2. Distribusi Barang per Kategori (Untuk Donut Chart)
    [HttpGet("category-distribution")]
    public async Task<IActionResult> GetCategoryDistribution()
    {
        var data = await _db.PartCategories
            .Select(c => new
            {
                CategoryName = c.Name,
                ItemCount = c.Parts.Count(p => p.IsActive)
            })
            .Where(x => x.ItemCount > 0) // Hanya tampilkan kategori yang ada barangnya
            .ToListAsync();

        return Ok(data);
    }

    // 3. Ambil Top 5 Barang dengan Stok Paling Menipis (Untuk Tabel Alert)
    [HttpGet("critical-stock")]
    public async Task<IActionResult> GetCriticalStock()
    {
        var data = await _db.Parts
            .Include(p => p.PartCategory)
            .Where(p => p.IsActive && p.Stock <= p.MinimumStock)
            .OrderBy(p => p.Stock) // Urutkan dari yang paling sedikit
            .Take(5)
            .Select(p => new
            {
                p.Id,
                p.PartNumber,
                p.Name,
                p.Stock,
                p.MinimumStock,
                CategoryName = p.PartCategory != null ? p.PartCategory.Name : "-"
            })
            .ToListAsync();

        return Ok(data);
    }
}