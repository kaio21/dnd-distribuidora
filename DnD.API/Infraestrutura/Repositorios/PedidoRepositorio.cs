using DnD.API.Dominio.Entidades;
using DnD.API.Dominio.Repositorios;
using DnD.API.Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Infraestrutura.Repositorios;

public class PedidoRepositorio : IPedidoRepositorio
{
    private readonly AppDbContext _db;

    public PedidoRepositorio(AppDbContext db) => _db = db;

    public async Task AdicionarAsync(Pedido pedido) =>
        await _db.Pedidos.AddAsync(pedido);

    public Task<Pedido?> ObterPorIdComDetalhesAsync(int id) =>
        _db.Pedidos
            .Include(p => p.Comprador)
            .Include(p => p.AtendidoPorVendedor)
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Pedido>> ListarAsync(
        int? compradorId = null,
        string? role = null,
        int? vendedorId = null,
        string? perfilVendedor = null,
        StatusPedido? status = null,
        DateTime? de = null,
        DateTime? ate = null)
    {
        var query = _db.Pedidos
            .Include(p => p.Comprador)
            .Include(p => p.AtendidoPorVendedor)
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .AsQueryable();

        if (role == "Buyer" && compradorId.HasValue)
            query = query.Where(p => p.CompradorId == compradorId.Value);
        else if (role == "Seller" && perfilVendedor == "Vendedor" && vendedorId.HasValue)
            query = query.Where(p => p.AtendidoPorVendedorId == null || p.AtendidoPorVendedorId == vendedorId.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (de.HasValue)
            query = query.Where(p => p.CriadoEm >= de.Value);

        if (ate.HasValue)
            query = query.Where(p => p.CriadoEm <= ate.Value.AddDays(1));

        return await query.OrderByDescending(p => p.CriadoEm).ToListAsync();
    }

    public async Task<IEnumerable<Pedido>> ObterTodosComItensAsync() =>
        await _db.Pedidos
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .Where(p => p.Status != StatusPedido.Cancelado)
            .ToListAsync();

    public Task SalvarAlteracoesAsync() =>
        _db.SaveChangesAsync();
}
