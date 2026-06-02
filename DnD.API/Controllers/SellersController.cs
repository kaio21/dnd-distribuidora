using DnD.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SellersController : ControllerBase
{
    private readonly AppDbContext _db;

    public SellersController(AppDbContext db) => _db = db;

    // Lista pública (id + nome) de vendedores aprovados, usada nos dropdowns
    // de "quem te atendeu" (checkout) e "quem te indicou" (cadastro).
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive()
    {
        var sellers = await _db.Sellers
            .Where(s => s.IsApproved)
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync();
        return Ok(sellers);
    }
}
