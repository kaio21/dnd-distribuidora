using DnD.API.Aplicacao.DTOs;
using DnD.API.Dominio;
using DnD.API.Dominio.Entidades;
using DnD.API.Dominio.Repositorios;

namespace DnD.API.Aplicacao.Servicos;

public class AdminServico
{
    private readonly IVendedorRepositorio _vendedorRepo;

    public AdminServico(IVendedorRepositorio vendedorRepo) =>
        _vendedorRepo = vendedorRepo;

    public async Task<IEnumerable<object>> ListarVendedoresAsync()
    {
        var vendedores = await _vendedorRepo.ListarTodosAsync();
        return vendedores.Select(v => new
        {
            v.Id, v.Nome, v.Email, v.CPF, v.Perfil, v.Aprovado, v.CriadoEm
        });
    }

    public async Task<object> CriarVendedorAsync(CriarVendedorDto dto)
    {
        var perfisPermitidos = new[] { "Vendedor", "Gerente", "Admin" };
        if (!perfisPermitidos.Contains(dto.Perfil))
            throw new DomainException("Perfil inválido. Use 'Vendedor', 'Gerente' ou 'Admin'.");

        var cpf = string.IsNullOrWhiteSpace(dto.CPF) ? null : dto.CPF;

        if (await _vendedorRepo.ExisteComEmailOuCpfAsync(dto.Email, cpf))
            throw new ConflitoDeDadosException("Email ou CPF já cadastrado.");

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
        var vendedor = Vendedor.Criar(dto.Nome, cpf, dto.Email, senhaHash, dto.Perfil);

        await _vendedorRepo.AdicionarAsync(vendedor);
        await _vendedorRepo.SalvarAlteracoesAsync();

        return new { vendedor.Id, vendedor.Nome, vendedor.Email, vendedor.Perfil };
    }

    public async Task<object?> RevogarAsync(int id)
    {
        var vendedor = await _vendedorRepo.ObterPorIdAsync(id);
        if (vendedor is null) return null;

        vendedor.Revogar(); // lança DomainException se for Admin
        await _vendedorRepo.SalvarAlteracoesAsync();

        return new { vendedor.Id, vendedor.Aprovado };
    }

    public async Task<object?> AprovarAsync(int id)
    {
        var vendedor = await _vendedorRepo.ObterPorIdAsync(id);
        if (vendedor is null) return null;

        vendedor.Aprovar();
        await _vendedorRepo.SalvarAlteracoesAsync();

        return new { vendedor.Id, vendedor.Aprovado };
    }
}
