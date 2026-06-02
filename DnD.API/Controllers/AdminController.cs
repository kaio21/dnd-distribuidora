using DnD.API.Data;
using DnD.API.DTOs;
using DnD.API.Models;
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
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Seller") return false;
        var sellerRole = User.FindFirstValue("SellerRole");
        // aceita tanto o novo claim quanto sellers antigos com IsAdmin no DB
        if (sellerRole == "Admin") return true;
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return _db.Sellers.Where(s => s.Id == id && s.Role == "Admin").Any();
    }

    [HttpGet("sellers")]
    public async Task<IActionResult> GetSellers()
    {
        if (!IsAdminSeller()) return Forbid();
        var sellers = await _db.Sellers
            .OrderBy(s => s.Role)
            .ThenBy(s => s.Name)
            .Select(s => new { s.Id, s.Name, s.Email, s.CPF, s.Role, s.IsApproved, s.CreatedAt })
            .ToListAsync();
        return Ok(sellers);
    }

    [HttpPost("sellers")]
    public async Task<IActionResult> CreateSeller([FromBody] CreateSellerDto dto)
    {
        if (!IsAdminSeller()) return Forbid();

        var cpf = string.IsNullOrWhiteSpace(dto.CPF) ? null : dto.CPF;
        if (await _db.Sellers.AnyAsync(s =>
            s.Email == dto.Email ||
            (cpf != null && s.CPF == cpf)))
            return Conflict(new { message = "Email ou CPF já cadastrado." });

        var allowed = new[] { "Vendedor", "Gerente", "Admin" };
        if (!allowed.Contains(dto.Role))
            return BadRequest(new { message = "Perfil inválido. Use 'Vendedor', 'Gerente' ou 'Admin'." });

        var seller = new Seller
        {
            Name = dto.Name,
            CPF = cpf,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            IsApproved = true
        };

        _db.Sellers.Add(seller);
        await _db.SaveChangesAsync();

        return Ok(new { seller.Id, seller.Name, seller.Email, seller.Role });
    }

    [HttpPatch("sellers/{id}/revoke")]
    public async Task<IActionResult> Revoke(int id)
    {
        if (!IsAdminSeller()) return Forbid();
        var seller = await _db.Sellers.FindAsync(id);
        if (seller == null) return NotFound();
        if (seller.Role == "Admin") return BadRequest(new { message = "Não é possível revogar o administrador." });
        seller.IsApproved = false;
        await _db.SaveChangesAsync();
        return Ok(new { seller.Id, seller.IsApproved });
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
}
