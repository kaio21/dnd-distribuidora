using DnD.API.Dominio.Entidades;

namespace DnD.API.Dominio.Repositorios;

public interface ICompradorRepositorio
{
    Task<Comprador?> ObterPorEmailAsync(string email);
    Task<Comprador?> ObterPorIdAsync(int id);
    Task<bool> ExisteComEmailCpfOuCnpjAsync(string email, string? cpf, string? cnpj);
    Task AdicionarAsync(Comprador comprador);
    Task SalvarAlteracoesAsync();
}
