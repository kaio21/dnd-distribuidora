using DnD.API.Data;
using DnD.API.DTOs;
using DnD.API.Hubs;
using DnD.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DnD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<ChatHub> _hub;

    public MessagesController(AppDbContext db, IHubContext<ChatHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendMessageDto dto)
    {
        var senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var senderRole = User.FindFirstValue(ClaimTypes.Role)!;
        var senderName = User.FindFirstValue(ClaimTypes.Name)!;

        var message = new Message
        {
            SenderId = senderId,
            SenderType = senderRole,
            ReceiverId = dto.ReceiverId,
            ReceiverType = dto.ReceiverType,
            Content = dto.Content
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        var response = new MessageResponseDto(
            message.Id, senderId, senderRole, senderName,
            dto.ReceiverId, dto.ReceiverType, dto.Content, false, message.CreatedAt);

        var buyerId = senderRole == "Buyer" ? senderId : dto.ReceiverId;
        await _hub.Clients.Group($"chat-{buyerId}").SendAsync("ReceiveMessage", response);

        return Ok(response);
    }

    [HttpGet("conversation/{buyerId}")]
    public async Task<IActionResult> GetConversation(int buyerId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        if (role == "Buyer" && userId != buyerId)
            return Forbid();

        var messages = await _db.Messages
            .Where(m =>
                (m.SenderType == "Buyer" && m.SenderId == buyerId) ||
                (m.ReceiverType == "Buyer" && m.ReceiverId == buyerId))
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        if (role == "Seller")
        {
            var unread = messages.Where(m => m.SenderType == "Buyer" && !m.IsRead).ToList();
            unread.ForEach(m => m.IsRead = true);
            if (unread.Any()) await _db.SaveChangesAsync();
        }

        var buyers = await _db.Buyers.ToDictionaryAsync(b => b.Id);
        var sellers = await _db.Sellers.ToDictionaryAsync(s => s.Id);

        return Ok(messages.Select(m => new MessageResponseDto(
            m.Id, m.SenderId, m.SenderType,
            m.SenderType == "Buyer"
                ? (buyers.TryGetValue(m.SenderId, out var b) ? b.Name : "Comprador")
                : (sellers.TryGetValue(m.SenderId, out var s) ? s.Name : "Vendedor"),
            m.ReceiverId, m.ReceiverType, m.Content, m.IsRead, m.CreatedAt)));
    }

    [HttpGet("conversations")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetConversations()
    {
        var messages = await _db.Messages
            .Include(m => m.Buyer)
            .ToListAsync();

        var buyerIds = messages
            .Select(m => m.SenderType == "Buyer" ? m.SenderId : m.ReceiverId)
            .Distinct();

        var buyers = await _db.Buyers.Where(b => buyerIds.Contains(b.Id)).ToListAsync();

        var conversations = buyers.Select(buyer =>
        {
            var buyerMessages = messages.Where(m =>
                (m.SenderType == "Buyer" && m.SenderId == buyer.Id) ||
                (m.ReceiverType == "Buyer" && m.ReceiverId == buyer.Id))
                .OrderByDescending(m => m.CreatedAt).ToList();

            var last = buyerMessages.FirstOrDefault();
            var unread = buyerMessages.Count(m => m.SenderType == "Buyer" && !m.IsRead);

            return new ConversationDto(
                buyer.Id, buyer.Name, buyer.StoreName, unread,
                last?.Content ?? "", last?.CreatedAt ?? DateTime.MinValue);
        }).OrderByDescending(c => c.LastMessageAt).ToList();

        return Ok(conversations);
    }
}
