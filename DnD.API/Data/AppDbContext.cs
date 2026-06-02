using DnD.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Seller> Sellers { get; set; }
    public DbSet<Buyer> Buyers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<StoreSettings> StoreSettings { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Seller>(e =>
        {
            e.HasIndex(s => s.CPF).IsUnique().HasFilter("\"CPF\" IS NOT NULL AND \"CPF\" != ''");
            e.HasIndex(s => s.Email).IsUnique();
            e.Property(s => s.CPF).HasMaxLength(14).IsRequired(false);
        });

        modelBuilder.Entity<Buyer>(e =>
        {
            e.HasIndex(b => b.CPF).IsUnique().HasFilter("\"CPF\" IS NOT NULL AND \"CPF\" != ''");
            e.HasIndex(b => b.CNPJ).IsUnique().HasFilter("\"CNPJ\" IS NOT NULL AND \"CNPJ\" != ''");
            e.HasIndex(b => b.Email).IsUnique();
            e.Property(b => b.CPF).HasMaxLength(14).IsRequired(false);
            e.Property(b => b.CNPJ).HasMaxLength(18).IsRequired(false);
            e.HasOne(b => b.ReferredBySeller)
             .WithMany()
             .HasForeignKey(b => b.ReferredBySellerId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.CostPrice).HasPrecision(10, 2);
            e.Property(p => p.SalePrice).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.Property(o => o.TotalAmount).HasPrecision(10, 2);
            e.Property(o => o.TotalProfit).HasPrecision(10, 2);
            e.Property(o => o.Status).HasConversion(
                v => v.ToString(),
                v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v));
            e.HasOne(o => o.Buyer)
             .WithMany(b => b.Orders)
             .HasForeignKey(o => o.BuyerId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.AttendedBySeller)
             .WithMany()
             .HasForeignKey(o => o.AttendedBySellerId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.Property(i => i.UnitPrice).HasPrecision(10, 2);
            e.Property(i => i.UnitCost).HasPrecision(10, 2);
            e.Property(i => i.Profit).HasPrecision(10, 2);
            e.HasOne(i => i.Order)
             .WithMany(o => o.Items)
             .HasForeignKey(i => i.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Product)
             .WithMany(p => p.OrderItems)
             .HasForeignKey(i => i.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Message>(e =>
        {
            e.Property(m => m.BuyerId).IsRequired(false);
            e.HasOne(m => m.Buyer)
             .WithMany(b => b.Messages)
             .HasForeignKey(m => m.BuyerId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
