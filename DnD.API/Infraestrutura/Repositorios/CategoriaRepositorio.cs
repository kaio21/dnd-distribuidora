using DnD.API.Dominio.Entidades;
using DnD.API.Dominio.Repositorios;
using DnD.API.Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Infraestrutura.Repositorios;

public class CategoriaRepositorio : ICategoriaRepositorio
{
    private readonly AppDbContext _db;

    public CategoriaRepositorio(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Categoria>> ListarAsync() =>
        await _db.Categorias.OrderBy(c => c.Nome).ToListAsync();

    public Task<bool> ExistePorNomeAsync(string nome) =>
        _db.Categorias.AnyAsync(c => c.Nome == nome);

    public Task<Categoria?> ObterPorIdAsync(int id) =>
        _db.Categorias.FindAsync(id).AsTask();

    public async Task AdicionarAsync(Categoria categoria) =>
        await _db.Categorias.AddAsync(categoria);

    public Task RemoverAsync(Categoria categoria)
    {
        _db.Categorias.Remove(categoria);
        return Task.CompletedTask;
    }

    public Task SalvarAlteracoesAsync() =>
        _db.SaveChangesAsync();
}
