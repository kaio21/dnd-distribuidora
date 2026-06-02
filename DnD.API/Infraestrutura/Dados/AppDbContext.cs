using DnD.API.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Infraestrutura.Dados;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Vendedor> Vendedores { get; set; }
    public DbSet<Comprador> Compradores { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<ItemPedido> ItensPedido { get; set; }
    public DbSet<Mensagem> Mensagens { get; set; }
    public DbSet<ConfiguracaoLoja> ConfiguracoesLoja { get; set; }
    public DbSet<Categoria> Categorias { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vendedor>(e =>
        {
            e.HasIndex(v => v.CPF).IsUnique().HasFilter("\"CPF\" IS NOT NULL AND \"CPF\" != ''");
            e.HasIndex(v => v.Email).IsUnique();
            e.Property(v => v.CPF).HasMaxLength(14).IsRequired(false);
        });

        modelBuilder.Entity<Comprador>(e =>
        {
            e.HasIndex(c => c.CPF).IsUnique().HasFilter("\"CPF\" IS NOT NULL AND \"CPF\" != ''");
            e.HasIndex(c => c.CNPJ).IsUnique().HasFilter("\"CNPJ\" IS NOT NULL AND \"CNPJ\" != ''");
            e.HasIndex(c => c.Email).IsUnique();
            e.Property(c => c.CPF).HasMaxLength(14).IsRequired(false);
            e.Property(c => c.CNPJ).HasMaxLength(18).IsRequired(false);
            e.HasOne(c => c.IndicadoPorVendedor)
             .WithMany()
             .HasForeignKey(c => c.IndicadoPorVendedorId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Produto>(e =>
        {
            e.Property(p => p.PrecoCusto).HasPrecision(10, 2);
            e.Property(p => p.PrecoVenda).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Pedido>(e =>
        {
            e.Property(p => p.ValorTotal).HasPrecision(10, 2);
            e.Property(p => p.LucroTotal).HasPrecision(10, 2);
            e.Property(p => p.Status).HasConversion(
                v => v.ToString(),
                v => (StatusPedido)Enum.Parse(typeof(StatusPedido), v));
            e.HasOne(p => p.Comprador)
             .WithMany(c => c.Pedidos)
             .HasForeignKey(p => p.CompradorId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.AtendidoPorVendedor)
             .WithMany()
             .HasForeignKey(p => p.AtendidoPorVendedorId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ItemPedido>(e =>
        {
            e.Property(i => i.PrecoUnitario).HasPrecision(10, 2);
            e.Property(i => i.CustoUnitario).HasPrecision(10, 2);
            e.Property(i => i.Lucro).HasPrecision(10, 2);
            e.HasOne(i => i.Pedido)
             .WithMany(p => p.Itens)
             .HasForeignKey(i => i.PedidoId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Produto)
             .WithMany(p => p.ItensPedido)
             .HasForeignKey(i => i.ProdutoId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Mensagem>(e =>
        {
            e.Property(m => m.CompradorId).IsRequired(false);
            e.HasOne(m => m.Comprador)
             .WithMany(c => c.Mensagens)
             .HasForeignKey(m => m.CompradorId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
