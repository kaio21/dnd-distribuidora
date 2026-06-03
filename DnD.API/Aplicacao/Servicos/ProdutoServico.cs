using DnD.API.Aplicacao.DTOs;
using DnD.API.Dominio.Entidades;
using DnD.API.Dominio.Repositorios;

namespace DnD.API.Aplicacao.Servicos;

public class ProdutoServico
{
    private readonly IProdutoRepositorio _produtoRepo;
    private readonly IArmazenamentoServico _armazenamento;

    public ProdutoServico(IProdutoRepositorio produtoRepo, IArmazenamentoServico armazenamento)
    {
        _produtoRepo = produtoRepo;
        _armazenamento = armazenamento;
    }

    public async Task<IEnumerable<RespostaProdutoDto>> ListarAsync(bool? apenasAtivos)
    {
        var produtos = await _produtoRepo.ListarAsync(apenasAtivos);
        return produtos.Select(MapearParaDto);
    }

    public async Task<RespostaProdutoDto?> ObterPorIdAsync(int id)
    {
        var produto = await _produtoRepo.ObterPorIdAsync(id);
        return produto is null ? null : MapearParaDto(produto);
    }

    public async Task<RespostaProdutoDto> CriarAsync(CriarProdutoDto dto, IFormFile? imagem)
    {
        var produto = Produto.Criar(dto.Name, dto.Description, dto.Category,
            dto.CostPrice, dto.SalePrice, dto.Stock);

        if (imagem != null)
            produto.DefinirImagem(await _armazenamento.EnviarAsync(imagem));

        await _produtoRepo.AdicionarAsync(produto);
        await _produtoRepo.SalvarAlteracoesAsync();

        return MapearParaDto(produto);
    }

    public async Task<RespostaProdutoDto?> AtualizarAsync(int id, AtualizarProdutoDto dto, IFormFile? imagem)
    {
        var produto = await _produtoRepo.ObterPorIdAsync(id);
        if (produto is null) return null;

        produto.Atualizar(dto.Name, dto.Description, dto.Category,
            dto.CostPrice, dto.SalePrice, dto.Stock, dto.IsActive);

        if (imagem != null)
        {
            var urlAntiga = produto.UrlImagem;
            produto.DefinirImagem(await _armazenamento.EnviarAsync(imagem));
            await _armazenamento.RemoverAsync(urlAntiga);
        }
        else if (dto.RemoveImage)
        {
            await _armazenamento.RemoverAsync(produto.UrlImagem);
            produto.RemoverImagem();
        }

        await _produtoRepo.SalvarAlteracoesAsync();
        return MapearParaDto(produto);
    }

    public async Task<bool?> AlternarAtividadeAsync(int id)
    {
        var produto = await _produtoRepo.ObterPorIdAsync(id);
        if (produto is null) return null;

        produto.AlternarAtividade();
        await _produtoRepo.SalvarAlteracoesAsync();
        return produto.Ativo;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var produto = await _produtoRepo.ObterPorIdAsync(id);
        if (produto is null) return false;

        await _produtoRepo.RemoverAsync(produto);
        await _produtoRepo.SalvarAlteracoesAsync();
        await _armazenamento.RemoverAsync(produto.UrlImagem);
        return true;
    }

    private static RespostaProdutoDto MapearParaDto(Produto p) => new(
        p.Id, p.Nome, p.Descricao, p.NomeCategoria,
        p.PrecoCusto, p.PrecoVenda, p.Lucro,
        p.Estoque, p.UrlImagem, p.Ativo, p.CriadoEm);
}
