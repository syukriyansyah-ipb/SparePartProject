using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoPartsApi.Data;
using AutoPartsApi.Models;

namespace AutoPartsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PartCategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public PartCategoriesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.PartCategories
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.Description })
            .ToListAsync();
        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,DealerAdmin")]
    public async Task<IActionResult> Create([FromBody] PartCategory category)
    {
        _db.PartCategories.Add(category);
        await _db.SaveChangesAsync();
        return Ok(new { category.Id, category.Name, category.Description });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,DealerAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] PartCategory updated)
    {
        var category = await _db.PartCategories.FindAsync(id);
        if (category == null) return NotFound();

        category.Name = updated.Name;
        category.Description = updated.Description;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,DealerAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.PartCategories.Include(c => c.Parts).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return NotFound();

        // Cegah hapus jika masih ada barang di kategori ini
        if (category.Parts.Any(p => p.IsActive))
            return BadRequest(new { message = "Tidak bisa hapus kategori, masih ada suku cadang aktif di dalamnya." });

        _db.PartCategories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}