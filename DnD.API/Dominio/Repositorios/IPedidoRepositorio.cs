using DnD.API.Dominio.Entidades;

namespace DnD.API.Dominio.Repositorios;

public interface IPedidoRepositorio
{
    Task AdicionarAsync(Pedido pedido);
    Task<Pedido?> ObterPorIdComDetalhesAsync(int id);
    Task<IEnumerable<Pedido>> ListarAsync(
        int? compradorId = null,
        string? role = null,
        int? vendedorId = null,
        string? perfilVendedor = null,
        StatusPedido? status = null,
        DateTime? de = null,
        DateTime? ate = null);
    Task<IEnumerable<Pedido>> ObterTodosComItensAsync();
    Task SalvarAlteracoesAsync();
}
