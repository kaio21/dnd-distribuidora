using DnD.API.Data;
using DnD.API.DTOs;
using DnD.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DnD.API.Hubs;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<ChatHub> _hub;
    private readonly IConfiguration _config;

    public OrdersController(AppDbContext db, IHubContext<ChatHub> hub, IConfiguration config)
    {
        _db = db;
        _hub = hub;
        _config = config;
    }

    [HttpPost]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var buyerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var buyer = await _db.Buyers.FindAsync(buyerId);
        if (buyer == null) return Unauthorized();

        var settings = await _db.StoreSettings.FirstOrDefaultAsync();
        if (settings != null)
        {
            if (!settings.IsOpen)
                return BadRequest(new { message = "A loja está fechada no momento. Tente mais tarde." });
            // IsOpen = true: aceita pedidos sem verificar horário
        }

        var order = new Order { BuyerId = buyerId };

        // afiliação opcional: vendedor que atendeu a venda
        if (dto.AttendedBySellerId.HasValue)
        {
            var sellerExists = await _db.Sellers.AnyAsync(s => s.Id == dto.AttendedBySellerId.Value && s.IsApproved);
            if (!sellerExists) return BadRequest(new { message = "Vendedor informado não encontrado." });
            order.AttendedBySellerId = dto.AttendedBySellerId;
        }

        var items = new List<OrderItem>();

        foreach (var itemDto in dto.Items)
        {
            var product = await _db.Products.FindAsync(itemDto.ProductId);
            if (product == null) return BadRequest(new { message = $"Produto {itemDto.ProductId} não encontrado." });
            if (!product.IsActive) return BadRequest(new { message = $"Produto '{product.Name}' está pausado." });
            if (product.Stock < itemDto.Quantity)
                return BadRequest(new { message = $"Estoque insuficiente para '{product.Name}'. Disponível: {product.Stock}" });

            var profit = (product.SalePrice - product.CostPrice) * itemDto.Quantity;
            items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.SalePrice,
                UnitCost = product.CostPrice,
                Profit = profit
            });

            product.Stock -= itemDto.Quantity;
            order.TotalAmount += product.SalePrice * itemDto.Quantity;
            order.TotalProfit += profit;
        }

        order.Items = items;
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        await _hub.Clients.Group("sellers").SendAsync("NewOrder", new { orderId = order.Id, buyerName = buyer.Name });

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, await BuildOrderResponse(order.Id));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var query = _db.Orders
            .Include(o => o.Buyer)
            .Include(o => o.AttendedBySeller)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .AsQueryable();

        if (role == "Buyer")
            query = query.Where(o => o.BuyerId == userId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var statusEnum))
            query = query.Where(o => o.Status == statusEnum);

        if (from.HasValue) query = query.Where(o => o.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(o => o.CreatedAt <= to.Value.AddDays(1));

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(orders.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await BuildOrderResponse(id);
        if (dto == null) return NotFound();
        return Ok(dto);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await _db.Orders.Include(o => o.Buyer).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();

        if (!Enum.TryParse<OrderStatus>(dto.Status, out var newStatus))
            return BadRequest(new { message = "Status inválido." });

        order.Status = newStatus;
        await _db.SaveChangesAsync();

        await _hub.Clients.Group($"buyer-{order.BuyerId}").SendAsync("OrderStatusUpdated", new
        {
            orderId = order.Id,
            status = dto.Status,
            buyerName = order.Buyer.Name
        });

        if (newStatus == OrderStatus.Pronto)
        {
            var whatsappNumber = order.Buyer.Phone.Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
            var message = Uri.EscapeDataString($"Olá {order.Buyer.Name}! Seu pedido #{order.Id} está PRONTO para retirada/entrega. Total: R$ {order.TotalAmount:F2}");
            var whatsappUrl = $"https://wa.me/{whatsappNumber}?text={message}";
            return Ok(new { status = dto.Status, whatsappUrl });
        }

        return Ok(new { status = dto.Status });
    }

    [HttpPatch("{id}/seller")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> AssignSeller(int id, [FromBody] AssignSellerDto dto)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        if (dto.AttendedBySellerId.HasValue)
        {
            var sellerExists = await _db.Sellers.AnyAsync(s => s.Id == dto.AttendedBySellerId.Value && s.IsApproved);
            if (!sellerExists) return BadRequest(new { message = "Vendedor informado não encontrado." });
        }

        order.AttendedBySellerId = dto.AttendedBySellerId;
        await _db.SaveChangesAsync();

        return Ok(await BuildOrderResponse(order.Id));
    }

    private async Task<OrderResponseDto?> BuildOrderResponse(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Buyer)
            .Include(o => o.AttendedBySeller)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return null;
        return MapToDto(order);
    }

    private static OrderResponseDto MapToDto(Order order) => new(
        order.Id, order.BuyerId,
        order.Buyer?.Name ?? "", order.Buyer?.StoreName ?? "",
        order.Buyer?.Phone ?? "",
        order.TotalAmount, order.TotalProfit,
        order.Status.ToString(), order.CreatedAt,
        order.AttendedBySellerId, order.AttendedBySeller?.Name,
        order.Items.Select(i => new OrderItemResponseDto(
            i.ProductId, i.Product?.Name ?? "", i.Quantity,
            i.UnitPrice, i.UnitCost, i.Profit)).ToList()
    );
}
