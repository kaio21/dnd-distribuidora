using DnD.API.Dominio.Entidades;

namespace DnD.API.Dominio.Repositorios;

public interface IProdutoRepositorio
{
    Task<IEnumerable<Produto>> ListarAsync(bool? apenasAtivos = null);
    Task<(int Total, int Ativos)> ContarAsync();
    Task<(IEnumerable<Produto> Itens, int TotalCount)> ListarPaginadoAsync(
        bool? apenasAtivos, string? busca, string? categoria, int pagina, int tamanhoPagina);
    Task<Produto?> ObterPorIdAsync(int id);
    Task<bool> ExisteProdutosNaCategoriaAsync(string nomeCategoria);
    Task RemoverItensPedidoAsync(int produtoId);
    Task AdicionarAsync(Produto produto);
    Task RemoverAsync(Produto produto);
    Task SalvarAlteracoesAsync();
}
