using DnD.API.Data;
using DnD.API.DTOs;
using DnD.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProductsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly)
    {
        var query = _db.Products.AsQueryable();
        if (activeOnly == true)
            query = query.Where(p => p.IsActive);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductResponseDto(
                p.Id, p.Name, p.Description, p.CostPrice, p.SalePrice,
                p.SalePrice - p.CostPrice, p.Stock, p.ImageUrl, p.IsActive, p.CreatedAt))
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();

        return Ok(new ProductResponseDto(
            p.Id, p.Name, p.Description, p.CostPrice, p.SalePrice,
            p.SalePrice - p.CostPrice, p.Stock, p.ImageUrl, p.IsActive, p.CreatedAt));
    }

    [HttpPost]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Create([FromForm] ProductCreateDto dto, IFormFile? image)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            CostPrice = dto.CostPrice,
            SalePrice = dto.SalePrice,
            Stock = dto.Stock
        };

        if (image != null)
            product.ImageUrl = await SaveImage(image);

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id },
            new ProductResponseDto(product.Id, product.Name, product.Description,
                product.CostPrice, product.SalePrice, product.SalePrice - product.CostPrice,
                product.Stock, product.ImageUrl, product.IsActive, product.CreatedAt));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Update(int id, [FromForm] ProductUpdateDto dto, IFormFile? image)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.CostPrice = dto.CostPrice;
        product.SalePrice = dto.SalePrice;
        product.Stock = dto.Stock;
        product.IsActive = dto.IsActive;

        if (image != null)
            product.ImageUrl = await SaveImage(image);

        await _db.SaveChangesAsync();
        return Ok(new ProductResponseDto(product.Id, product.Name, product.Description,
            product.CostPrice, product.SalePrice, product.SalePrice - product.CostPrice,
            product.Stock, product.ImageUrl, product.IsActive, product.CreatedAt));
    }

    [HttpPatch("{id}/toggle")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Toggle(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.IsActive = !product.IsActive;
        await _db.SaveChangesAsync();
        return Ok(new { isActive = product.IsActive });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string> SaveImage(IFormFile file)
    {
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{fileName}";
    }
}
