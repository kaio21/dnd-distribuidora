using DnD.API.Dominio.Entidades;

namespace DnD.API.Dominio.Repositorios;

public interface ICategoriaRepositorio
{
    Task<IEnumerable<Categoria>> ListarAsync();
    Task<bool> ExistePorNomeAsync(string nome);
    Task<Categoria?> ObterPorIdAsync(int id);
    Task AdicionarAsync(Categoria categoria);
    Task RemoverAsync(Categoria categoria);
    Task SalvarAlteracoesAsync();
}
