using DnD.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BuyersController : ControllerBase
{
    private readonly AppDbContext _db;

    public BuyersController(AppDbContext db) => _db = db;

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var buyer = await _db.Buyers.FindAsync(id);
        if (buyer == null) return NotFound();

        return Ok(new
        {
            buyer.Id,
            buyer.Name,
            buyer.StoreName,
            buyer.CPF,
            buyer.CNPJ,
            buyer.Phone,
            buyer.Address,
            buyer.Email
        });
    }
}
