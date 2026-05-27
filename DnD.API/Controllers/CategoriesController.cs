using DnD.API.Data;
using DnD.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cats = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        return Ok(cats.Select(c => new { c.Id, c.Name }));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest req)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
        if (role != "Seller") return Forbid();

        var name = req.Name?.Trim();
        if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Nome inválido." });

        if (await _db.Categories.AnyAsync(c => c.Name == name))
            return Conflict(new { message = "Categoria já existe." });

        var cat = new Category { Name = name };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return Ok(new { cat.Id, cat.Name });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
        if (role != "Seller") return Forbid();

        var cat = await _db.Categories.FindAsync(id);
        if (cat == null) return NotFound();

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record CreateCategoryRequest(string Name);
