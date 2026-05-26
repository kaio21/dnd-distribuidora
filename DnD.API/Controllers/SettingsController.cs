using DnD.API.Data;
using DnD.API.DTOs;
using DnD.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var s = await _db.StoreSettings.FirstOrDefaultAsync()
            ?? new StoreSettings();
        return Ok(new StoreSettingsDto(s.OpenTime, s.CloseTime, s.IsOpen));
    }

    [HttpPut]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Update([FromBody] StoreSettingsDto dto)
    {
        var s = await _db.StoreSettings.FirstOrDefaultAsync();
        if (s == null) { s = new StoreSettings(); _db.StoreSettings.Add(s); }

        s.OpenTime = dto.OpenTime;
        s.CloseTime = dto.CloseTime;
        s.IsOpen = dto.IsOpen;
        await _db.SaveChangesAsync();
        return Ok(new StoreSettingsDto(s.OpenTime, s.CloseTime, s.IsOpen));
    }
}
