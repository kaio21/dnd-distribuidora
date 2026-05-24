using DnD.API.Data;
using DnD.API.DTOs;
using DnD.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Seller")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard([FromQuery] string period = "weekly")
    {
        var orders = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.Status != OrderStatus.Cancelado)
            .ToListAsync();

        var totalRevenue = orders.Sum(o => o.TotalAmount);
        var totalProfit = orders.Sum(o => o.TotalProfit);
        var totalOrders = orders.Count;
        var totalProducts = await _db.Products.CountAsync();
        var activeProducts = await _db.Products.CountAsync(p => p.IsActive);

        var summary = new DashboardSummaryDto(totalRevenue, totalProfit, totalOrders, totalProducts, activeProducts);

        var now = DateTime.UtcNow;
        var dailyData = Enumerable.Range(0, 7).Select(i =>
        {
            var date = now.AddDays(-i).Date;
            var dayOrders = orders.Where(o => o.CreatedAt.Date == date).ToList();
            return new SalesPeriodDto(
                date.ToString("dd/MM"),
                dayOrders.Sum(o => o.TotalAmount),
                dayOrders.Sum(o => o.TotalProfit),
                dayOrders.Count);
        }).Reverse().ToList();

        var weeklyData = Enumerable.Range(0, 8).Select(i =>
        {
            var weekStart = now.AddDays(-(i * 7) - (int)now.DayOfWeek).Date;
            var weekEnd = weekStart.AddDays(7);
            var weekOrders = orders.Where(o => o.CreatedAt.Date >= weekStart && o.CreatedAt.Date < weekEnd).ToList();
            return new SalesPeriodDto(
                $"Sem {weekStart:dd/MM}",
                weekOrders.Sum(o => o.TotalAmount),
                weekOrders.Sum(o => o.TotalProfit),
                weekOrders.Count);
        }).Reverse().ToList();

        var topProducts = orders
            .SelectMany(o => o.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new TopProductDto(
                g.First().Product?.Name ?? "Produto",
                g.Sum(i => i.Quantity),
                g.Sum(i => i.UnitPrice * i.Quantity),
                g.Sum(i => i.Profit)))
            .OrderByDescending(p => p.QuantitySold)
            .Take(5)
            .ToList();

        return Ok(new DashboardResponseDto(summary, dailyData, weeklyData, topProducts));
    }

    [HttpGet("sales")]
    public async Task<IActionResult> GetSales([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? groupBy = "day")
    {
        var query = _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Buyer)
            .Where(o => o.Status != OrderStatus.Cancelado)
            .AsQueryable();

        if (from.HasValue) query = query.Where(o => o.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(o => o.CreatedAt <= to.Value.AddDays(1));

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

        return Ok(orders.Select(o => new
        {
            o.Id,
            BuyerName = o.Buyer?.Name,
            BuyerStore = o.Buyer?.StoreName,
            o.TotalAmount,
            o.TotalProfit,
            o.Status,
            o.CreatedAt,
            Items = o.Items.Select(i => new
            {
                i.ProductId,
                ProductName = i.Product?.Name,
                i.Quantity,
                i.UnitPrice,
                i.Profit
            })
        }));
    }
}
