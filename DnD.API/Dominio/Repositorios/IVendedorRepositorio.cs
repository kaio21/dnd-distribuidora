using DnD.API.Dominio.Entidades;

namespace DnD.API.Dominio.Repositorios;

public interface IVendedorRepositorio
{
    Task<Vendedor?> ObterPorEmailAsync(string email);
    Task<Vendedor?> ObterPorIdAsync(int id);
    Task<bool> ExisteComEmailOuCpfAsync(string email, string? cpf);
    Task<bool> ExisteAprovadoPorIdAsync(int id);
    Task<IEnumerable<Vendedor>> ListarAprovadosAsync();
    Task<IEnumerable<Vendedor>> ListarTodosAsync();
    Task AdicionarAsync(Vendedor vendedor);
    Task SalvarAlteracoesAsync();
}
