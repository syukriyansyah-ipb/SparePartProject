using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoPartsApi.Data;
using AutoPartsApi.Models;

namespace AutoPartsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly AppDbContext _db;

    public SuppliersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var suppliers = await _db.Suppliers
            .Where(s => s.IsActive)
            .Select(s => new { s.Id, s.Name, s.ContactPerson, s.Phone, s.Address })
            .ToListAsync();
        return Ok(suppliers);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,DealerAdmin")]
    public async Task<IActionResult> Create([FromBody] Supplier supplier)
    {
        supplier.IsActive = true;
        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();
        return Ok(new { supplier.Id, supplier.Name, supplier.ContactPerson, supplier.Phone, supplier.Address });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,DealerAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] Supplier updated)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();

        supplier.Name = updated.Name;
        supplier.ContactPerson = updated.ContactPerson;
        supplier.Phone = updated.Phone;
        supplier.Address = updated.Address;
        
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,DealerAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();

        supplier.IsActive = false; // Soft delete
        await _db.SaveChangesAsync();
        return NoContent();
    }
}