namespace DnD.API.Models;

public class Seller
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    // "Admin", "Gerente", "Vendedor"
    public string Role { get; set; } = "Vendedor";
    public bool IsApproved { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
