using DnD.API.Dominio.Entidades;
using DnD.API.Dominio.Repositorios;
using DnD.API.Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Infraestrutura.Repositorios;

public class CompradorRepositorio : ICompradorRepositorio
{
    private readonly AppDbContext _db;

    public CompradorRepositorio(AppDbContext db) => _db = db;

    public Task<Comprador?> ObterPorEmailAsync(string email) =>
        _db.Compradores.FirstOrDefaultAsync(c => c.Email == email);

    public Task<Comprador?> ObterPorIdAsync(int id) =>
        _db.Compradores.FindAsync(id).AsTask();

    public Task<bool> ExisteComEmailCpfOuCnpjAsync(string email, string? cpf, string? cnpj) =>
        _db.Compradores.AnyAsync(c =>
            c.Email == email ||
            (cpf  != null && c.CPF  == cpf)  ||
            (cnpj != null && c.CNPJ == cnpj));

    public async Task AdicionarAsync(Comprador comprador) =>
        await _db.Compradores.AddAsync(comprador);

    public Task SalvarAlteracoesAsync() =>
        _db.SaveChangesAsync();
}
