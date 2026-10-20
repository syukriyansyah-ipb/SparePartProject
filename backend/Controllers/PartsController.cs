using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoPartsApi.Data;
using AutoPartsApi.Models;

namespace AutoPartsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PartsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PartsController(AppDbContext db)
    {
        _db = db;
    }

    // 1. GET ALL
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? lowStock)
    {
        var query = _db.Parts
            .Include(p => p.PartCategory)
            .Include(p => p.CompatibleVehicles)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search) || p.PartNumber.Contains(search));

        if (lowStock.HasValue && lowStock.Value)
            query = query.Where(p => p.Stock <= p.MinimumStock);

        // GUNAKAN SELECT UNTUK MENCEGAH OBJECT CYCLE
        var parts = await query.OrderBy(p => p.Name)
            .Select(p => new 
            {
                p.Id,
                p.PartNumber,
                p.Name,
                p.Price,
                p.Stock,
                p.MinimumStock,
                PartCategory = p.PartCategory == null ? null : new 
                {
                    p.PartCategory.Id,
                    p.PartCategory.Name
                },
                CompatibleVehicles = p.CompatibleVehicles.Select(v => new 
                {
                    v.Id,
                    v.Name
                }).ToList()
            })
            .ToListAsync();

        return Ok(parts);
    }

    // 2. CREATE
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,DealerAdmin")]
    public async Task<IActionResult> Create([FromBody] Part part)
    {
        if (await _db.Parts.AnyAsync(p => p.PartNumber == part.PartNumber))
            return BadRequest(new { message = "Kode Part sudah digunakan" });

        part.CreatedAt = DateTime.UtcNow;
        _db.Parts.Add(part);
        await _db.SaveChangesAsync();
        
        // Kembalikan data yang aman (tanpa Include)
        return Ok(new 
        {
            part.Id,
            part.PartNumber,
            part.Name,
            part.Price,
            part.Stock,
            part.MinimumStock,
            part.PartCategoryId
        });
    }

    // 3. UPDATE STOK
    [HttpPatch("{id}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] int quantity)
    {
        var part = await _db.Parts.FindAsync(id);
        if (part == null) return NotFound(new { message = "Part tidak ditemukan" });

        part.Stock += quantity;
        
        if (part.Stock < 0)
            return BadRequest(new { message = "Stok tidak mencukupi" });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Stok berhasil diupdate", currentStock = part.Stock });
    }
    
    // 4. DELETE
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,DealerAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var part = await _db.Parts.FindAsync(id);
        if (part == null) return NotFound();

        part.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}