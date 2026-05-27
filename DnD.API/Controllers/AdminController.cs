using DnD.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db) => _db = db;

    private bool IsAdminSeller()
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
        if (role != "Seller") return false;
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return _db.Sellers.Where(s => s.Id == id && s.IsAdmin).Any();
    }

    [HttpGet("sellers")]
    public async Task<IActionResult> GetSellers()
    {
        if (!IsAdminSeller()) return Forbid();
        var sellers = await _db.Sellers
            .OrderByDescending(s => s.IsAdmin)
            .ThenBy(s => s.Name)
            .Select(s => new { s.Id, s.Name, s.Email, s.CPF, s.IsAdmin, s.IsApproved, s.CreatedAt })
            .ToListAsync();
        return Ok(sellers);
    }

    [HttpPatch("sellers/{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        if (!IsAdminSeller()) return Forbid();
        var seller = await _db.Sellers.FindAsync(id);
        if (seller == null) return NotFound();
        seller.IsApproved = true;
        await _db.SaveChangesAsync();
        return Ok(new { seller.Id, seller.IsApproved });
    }

    [HttpPatch("sellers/{id}/revoke")]
    public async Task<IActionResult> Revoke(int id)
    {
        if (!IsAdminSeller()) return Forbid();
        var seller = await _db.Sellers.FindAsync(id);
        if (seller == null) return NotFound();
        if (seller.IsAdmin) return BadRequest(new { message = "Não é possível revogar o administrador." });
        seller.IsApproved = false;
        await _db.SaveChangesAsync();
        return Ok(new { seller.Id, seller.IsApproved });
    }
}
