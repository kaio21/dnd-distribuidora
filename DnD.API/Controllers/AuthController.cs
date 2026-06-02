using DnD.API.Data;
using DnD.API.DTOs;
using DnD.API.Models;
using DnD.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("seller/register")]
    public async Task<IActionResult> SellerRegister([FromBody] SellerRegisterDto dto)
    {
        if (await _db.Sellers.AnyAsync(s => s.Email == dto.Email || s.CPF == dto.CPF))
            return Conflict(new { message = "Email ou CPF já cadastrado." });

        var hasAdmin = await _db.Sellers.AnyAsync(s => s.IsAdmin);
        var seller = new Seller
        {
            Name = dto.Name,
            CPF = dto.CPF,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsApproved = !hasAdmin // primeiro vendedor aprovado automaticamente; demais ficam pendentes
        };

        _db.Sellers.Add(seller);
        await _db.SaveChangesAsync();

        if (!seller.IsApproved)
            return Ok(new { pending = true, message = "Cadastro realizado! Aguarde aprovação do administrador para acessar." });

        var token = _jwt.GenerateToken(seller.Id, seller.Email, "Seller", seller.Name);
        return Ok(new AuthResponseDto(token, "Seller", seller.Id, seller.Name, seller.Email, seller.IsAdmin));
    }

    [HttpPost("seller/login")]
    public async Task<IActionResult> SellerLogin([FromBody] SellerLoginDto dto)
    {
        var seller = await _db.Sellers.FirstOrDefaultAsync(s => s.Email == dto.Email);
        if (seller == null || !BCrypt.Net.BCrypt.Verify(dto.Password, seller.PasswordHash))
            return Unauthorized(new { message = "Email ou senha inválidos." });

        if (!seller.IsApproved)
            return Unauthorized(new { message = "Conta ainda não aprovada. Aguarde o administrador." });

        var token = _jwt.GenerateToken(seller.Id, seller.Email, "Seller", seller.Name);
        return Ok(new AuthResponseDto(token, "Seller", seller.Id, seller.Name, seller.Email, seller.IsAdmin));
    }

    [HttpPost("buyer/register")]
    public async Task<IActionResult> BuyerRegister([FromBody] BuyerRegisterDto dto)
    {
        if (await _db.Buyers.AnyAsync(b => b.Email == dto.Email || b.CPF == dto.CPF || b.CNPJ == dto.CNPJ))
            return Conflict(new { message = "Email, CPF ou CNPJ já cadastrado." });

        // afiliação opcional: vendedor que indicou o comprador
        int? referredBy = null;
        if (dto.ReferredBySellerId.HasValue &&
            await _db.Sellers.AnyAsync(s => s.Id == dto.ReferredBySellerId.Value && s.IsApproved))
        {
            referredBy = dto.ReferredBySellerId;
        }

        var buyer = new Buyer
        {
            Name = dto.Name,
            StoreName = dto.StoreName,
            CPF = dto.CPF,
            CNPJ = dto.CNPJ,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Phone = dto.Phone,
            Address = dto.Address,
            ReferredBySellerId = referredBy
        };

        _db.Buyers.Add(buyer);
        await _db.SaveChangesAsync();

        var token = _jwt.GenerateToken(buyer.Id, buyer.Email, "Buyer", buyer.Name);
        return Ok(new AuthResponseDto(token, "Buyer", buyer.Id, buyer.Name, buyer.Email));
    }

    [HttpPost("buyer/login")]
    public async Task<IActionResult> BuyerLogin([FromBody] BuyerLoginDto dto)
    {
        var buyer = await _db.Buyers.FirstOrDefaultAsync(b => b.Email == dto.Email);
        if (buyer == null || !BCrypt.Net.BCrypt.Verify(dto.Password, buyer.PasswordHash))
            return Unauthorized(new { message = "Email ou senha inválidos." });

        var token = _jwt.GenerateToken(buyer.Id, buyer.Email, "Buyer", buyer.Name);
        return Ok(new AuthResponseDto(token, "Buyer", buyer.Id, buyer.Name, buyer.Email));
    }
}
