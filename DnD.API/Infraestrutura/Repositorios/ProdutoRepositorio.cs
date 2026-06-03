using DnD.API.Dominio.Entidades;
using DnD.API.Dominio.Repositorios;
using DnD.API.Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Infraestrutura.Repositorios;

public class ProdutoRepositorio : IProdutoRepositorio
{
    private readonly AppDbContext _db;

    public ProdutoRepositorio(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Produto>> ListarAsync(bool? apenasAtivos = null)
    {
        var query = _db.Produtos.AsQueryable();

        if (apenasAtivos == true)
            query = query.Where(p => p.Ativo);

        return await query.OrderByDescending(p => p.CriadoEm).ToListAsync();
    }

    public Task<Produto?> ObterPorIdAsync(int id) =>
        _db.Produtos.FindAsync(id).AsTask();

    public Task<bool> ExisteProdutosNaCategoriaAsync(string nomeCategoria) =>
        _db.Produtos.AnyAsync(p => p.NomeCategoria == nomeCategoria);

    public async Task RemoverItensPedidoAsync(int produtoId)
    {
        var itens = await _db.ItensPedido.Where(i => i.ProdutoId == produtoId).ToListAsync();
        if (itens.Count > 0)
            _db.ItensPedido.RemoveRange(itens);
    }

    public async Task AdicionarAsync(Produto produto) =>
        await _db.Produtos.AddAsync(produto);

    public Task RemoverAsync(Produto produto)
    {
        _db.Produtos.Remove(produto);
        return Task.CompletedTask;
    }

    public Task SalvarAlteracoesAsync() =>
        _db.SaveChangesAsync();
}
