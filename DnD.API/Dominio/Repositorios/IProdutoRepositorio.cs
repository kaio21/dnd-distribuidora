using DnD.API.Dominio.Entidades;

namespace DnD.API.Dominio.Repositorios;

public interface IProdutoRepositorio
{
    Task<IEnumerable<Produto>> ListarAsync(bool? apenasAtivos = null);
    Task<Produto?> ObterPorIdAsync(int id);
    Task AdicionarAsync(Produto produto);
    Task RemoverAsync(Produto produto);
    Task SalvarAlteracoesAsync();
}
