namespace DnD.API.Models;

public class Message
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public int ReceiverId { get; set; }
    public string ReceiverType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? BuyerId { get; set; }
    public Buyer? Buyer { get; set; }
}
