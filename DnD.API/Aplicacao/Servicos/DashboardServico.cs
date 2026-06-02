using DnD.API.Aplicacao.DTOs;
using DnD.API.Dominio.Repositorios;

namespace DnD.API.Aplicacao.Servicos;

public class DashboardServico
{
    private readonly IPedidoRepositorio _pedidoRepo;
    private readonly IProdutoRepositorio _produtoRepo;

    public DashboardServico(IPedidoRepositorio pedidoRepo, IProdutoRepositorio produtoRepo)
    {
        _pedidoRepo  = pedidoRepo;
        _produtoRepo = produtoRepo;
    }

    public async Task<RespostaDashboardDto> ObterDashboardAsync()
    {
        var pedidos = (await _pedidoRepo.ObterTodosComItensAsync()).ToList();
        var todosOsProdutos = (await _produtoRepo.ListarAsync()).ToList();

        var resumo = new ResumoDashboardDto(
            pedidos.Sum(p => p.ValorTotal),
            pedidos.Sum(p => p.LucroTotal),
            pedidos.Count,
            todosOsProdutos.Count,
            todosOsProdutos.Count(p => p.Ativo));

        var agora = DateTime.UtcNow;

        var dadosDiarios = Enumerable.Range(0, 7).Select(i =>
        {
            var data = agora.AddDays(-i).Date;
            var pedidosDia = pedidos.Where(p => p.CriadoEm.Date == data).ToList();
            return new PeriodoVendasDto(
                data.ToString("dd/MM"),
                pedidosDia.Sum(p => p.ValorTotal),
                pedidosDia.Sum(p => p.LucroTotal),
                pedidosDia.Count);
        }).Reverse().ToList();

        var dadosSemanais = Enumerable.Range(0, 8).Select(i =>
        {
            var inicioSemana = agora.AddDays(-(i * 7) - (int)agora.DayOfWeek).Date;
            var fimSemana = inicioSemana.AddDays(7);
            var pedidosSemana = pedidos
                .Where(p => p.CriadoEm.Date >= inicioSemana && p.CriadoEm.Date < fimSemana)
                .ToList();
            return new PeriodoVendasDto(
                $"Sem {inicioSemana:dd/MM}",
                pedidosSemana.Sum(p => p.ValorTotal),
                pedidosSemana.Sum(p => p.LucroTotal),
                pedidosSemana.Count);
        }).Reverse().ToList();

        var topProdutos = pedidos
            .SelectMany(p => p.Itens)
            .GroupBy(i => i.ProdutoId)
            .Select(g => new TopProdutoDto(
                g.First().Produto?.Nome ?? "Produto",
                g.Sum(i => i.Quantidade),
                g.Sum(i => i.PrecoUnitario * i.Quantidade),
                g.Sum(i => i.Lucro)))
            .OrderByDescending(p => p.QuantidadeVendida)
            .Take(5)
            .ToList();

        return new RespostaDashboardDto(resumo, dadosDiarios, dadosSemanais, topProdutos);
    }

    public async Task<IEnumerable<object>> ObterVendasAsync(DateTime? de, DateTime? ate)
    {
        var pedidos = await _pedidoRepo.ListarAsync(de: de, ate: ate);

        return pedidos.Select(p => new
        {
            p.Id,
            NomeComprador = p.Comprador?.Nome,
            NomeLoja = p.Comprador?.NomeLoja,
            p.ValorTotal,
            p.LucroTotal,
            Status = p.Status.ToString(),
            p.CriadoEm,
            Itens = p.Itens.Select(i => new
            {
                i.ProdutoId,
                NomeProduto = i.Produto?.Nome,
                i.Quantidade,
                i.PrecoUnitario,
                i.Lucro
            })
        });
    }
}
