using DnD.API.Dominio.Entidades;
using DnD.API.Dominio.Repositorios;
using DnD.API.Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Infraestrutura.Repositorios;

public class VendedorRepositorio : IVendedorRepositorio
{
    private readonly AppDbContext _db;

    public VendedorRepositorio(AppDbContext db) => _db = db;

    public Task<Vendedor?> ObterPorEmailAsync(string email) =>
        _db.Vendedores.FirstOrDefaultAsync(v => v.Email == email);

    public Task<Vendedor?> ObterPorIdAsync(int id) =>
        _db.Vendedores.FindAsync(id).AsTask();

    public Task<bool> ExisteComEmailOuCpfAsync(string email, string? cpf) =>
        _db.Vendedores.AnyAsync(v =>
            v.Email == email ||
            (cpf != null && v.CPF == cpf));

    public Task<bool> ExisteAprovadoPorIdAsync(int id) =>
        _db.Vendedores.AnyAsync(v => v.Id == id && v.Aprovado);

    public async Task<IEnumerable<Vendedor>> ListarAprovadosAsync() =>
        await _db.Vendedores
            .Where(v => v.Aprovado)
            .OrderBy(v => v.Nome)
            .ToListAsync();

    public async Task<IEnumerable<Vendedor>> ListarTodosAsync() =>
        await _db.Vendedores
            .OrderBy(v => v.Perfil)
            .ThenBy(v => v.Nome)
            .ToListAsync();

    public async Task AdicionarAsync(Vendedor vendedor) =>
        await _db.Vendedores.AddAsync(vendedor);

    public Task SalvarAlteracoesAsync() =>
        _db.SaveChangesAsync();
}
