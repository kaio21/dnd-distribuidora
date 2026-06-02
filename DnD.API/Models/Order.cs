namespace DnD.API.Models;

public enum OrderStatus
{
    Pendente,
    Confirmado,
    Pronto,
    Entregue,
    Cancelado
}

public class Order
{
    public int Id { get; set; }
    public int BuyerId { get; set; }
    public Buyer Buyer { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal TotalProfit { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pendente;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Vendedor que atendeu a venda (afiliação opcional)
    public int? AttendedBySellerId { get; set; }
    public Seller? AttendedBySeller { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
