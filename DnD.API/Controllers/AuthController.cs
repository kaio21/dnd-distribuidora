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

        var seller = new Seller
        {
            Name = dto.Name,
            CPF = dto.CPF,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _db.Sellers.Add(seller);
        await _db.SaveChangesAsync();

        var token = _jwt.GenerateToken(seller.Id, seller.Email, "Seller", seller.Name);
        return Ok(new AuthResponseDto(token, "Seller", seller.Id, seller.Name, seller.Email));
    }

    [HttpPost("seller/login")]
    public async Task<IActionResult> SellerLogin([FromBody] SellerLoginDto dto)
    {
        var seller = await _db.Sellers.FirstOrDefaultAsync(s => s.Email == dto.Email);
        if (seller == null || !BCrypt.Net.BCrypt.Verify(dto.Password, seller.PasswordHash))
            return Unauthorized(new { message = "Email ou senha inválidos." });

        var token = _jwt.GenerateToken(seller.Id, seller.Email, "Seller", seller.Name);
        return Ok(new AuthResponseDto(token, "Seller", seller.Id, seller.Name, seller.Email));
    }

    [HttpPost("buyer/register")]
    public async Task<IActionResult> BuyerRegister([FromBody] BuyerRegisterDto dto)
    {
        if (await _db.Buyers.AnyAsync(b => b.Email == dto.Email || b.CPF == dto.CPF || b.CNPJ == dto.CNPJ))
            return Conflict(new { message = "Email, CPF ou CNPJ já cadastrado." });

        var buyer = new Buyer
        {
            Name = dto.Name,
            StoreName = dto.StoreName,
            CPF = dto.CPF,
            CNPJ = dto.CNPJ,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Phone = dto.Phone,
            Address = dto.Address
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
