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
    Task<(IEnumerable<Pedido> Itens, int TotalCount)> ListarPaginadoAsync(
        int? compradorId, string? role, int? vendedorId, string? perfilVendedor,
        StatusPedido? status, DateTime? de, DateTime? ate, int pagina, int tamanhoPagina);
    Task<(decimal TotalValor, decimal TotalLucro)> ObterTotaisAsync(
        int? compradorId, string? role, int? vendedorId, string? perfilVendedor,
        StatusPedido? status, DateTime? de, DateTime? ate);
    Task<IEnumerable<Pedido>> ObterTodosComItensAsync();
    Task<IEnumerable<Pedido>> ObterRecentesComItensAsync(DateTime desde);
    Task<(decimal TotalValor, decimal TotalLucro, int TotalCount)> ObterResumoGeralAsync();
    Task SalvarAlteracoesAsync();
}
