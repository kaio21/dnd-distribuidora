using DnD.API.Dominio;
using DnD.API.Dominio.Entidades;
using DnD.API.Dominio.Repositorios;

namespace DnD.API.Aplicacao.Servicos;

public class CategoriaServico
{
    private readonly ICategoriaRepositorio _categoriaRepo;

    public CategoriaServico(ICategoriaRepositorio categoriaRepo) =>
        _categoriaRepo = categoriaRepo;

    public async Task<IEnumerable<object>> ListarAsync()
    {
        var categorias = await _categoriaRepo.ListarAsync();
        return categorias.Select(c => new { c.Id, Name = c.Nome });
    }

    public async Task<object> CriarAsync(string nome)
    {
        var nomeTratado = nome?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(nomeTratado))
            throw new DomainException("Nome da categoria inválido.");

        if (await _categoriaRepo.ExistePorNomeAsync(nomeTratado))
            throw new DomainException("Categoria já existe.");

        var categoria = Categoria.Criar(nomeTratado);
        await _categoriaRepo.AdicionarAsync(categoria);
        await _categoriaRepo.SalvarAlteracoesAsync();

        return new { categoria.Id, Name = categoria.Nome };
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var categoria = await _categoriaRepo.ObterPorIdAsync(id);
        if (categoria is null) return false;

        await _categoriaRepo.RemoverAsync(categoria);
        await _categoriaRepo.SalvarAlteracoesAsync();
        return true;
    }
}
