namespace DnD.API.Models;

public class Buyer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? CPF { get; set; }
    public string? CNPJ { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Vendedor que indicou este comprador (afiliação opcional)
    public int? ReferredBySellerId { get; set; }
    public Seller? ReferredBySeller { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
