using DnD.API.Aplicacao.DTOs;
using DnD.API.Dominio;
using DnD.API.Dominio.Entidades;
using DnD.API.Dominio.Repositorios;
using DnD.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Aplicacao.Servicos;

public class PedidoServico
{
    private readonly IPedidoRepositorio _pedidoRepo;
    private readonly IProdutoRepositorio _produtoRepo;
    private readonly IVendedorRepositorio _vendedorRepo;
    private readonly ICompradorRepositorio _compradorRepo;
    private readonly IConfiguracaoLojaRepositorio _configRepo;
    private readonly IHubContext<ChatHub> _hub;

    public PedidoServico(
        IPedidoRepositorio pedidoRepo,
        IProdutoRepositorio produtoRepo,
        IVendedorRepositorio vendedorRepo,
        ICompradorRepositorio compradorRepo,
        IConfiguracaoLojaRepositorio configRepo,
        IHubContext<ChatHub> hub)
    {
        _pedidoRepo  = pedidoRepo;
        _produtoRepo = produtoRepo;
        _vendedorRepo = vendedorRepo;
        _compradorRepo = compradorRepo;
        _configRepo  = configRepo;
        _hub = hub;
    }

    public async Task<RespostaPedidoDto> CriarAsync(CriarPedidoDto dto, int compradorId)
    {
        var comprador = await _compradorRepo.ObterPorIdAsync(compradorId)
            ?? throw new DomainException("Comprador não encontrado.");

        var config = await _configRepo.ObterAsync();
        if (config is not null && !config.Aberta)
            throw new DomainException("A loja está fechada no momento. Tente mais tarde.");

        if (dto.AtendidoPorVendedorId.HasValue)
        {
            var vendedorValido = await _vendedorRepo.ExisteAprovadoPorIdAsync(dto.AtendidoPorVendedorId.Value);
            if (!vendedorValido)
                throw new DomainException("Vendedor informado não encontrado.");
        }

        var pedido = Pedido.Criar(compradorId, dto.AtendidoPorVendedorId);

        foreach (var itemDto in dto.Itens)
        {
            var produto = await _produtoRepo.ObterPorIdAsync(itemDto.ProdutoId)
                ?? throw new DomainException($"Produto {itemDto.ProdutoId} não encontrado.");

            pedido.AdicionarItem(produto, itemDto.Quantidade);
        }

        await _pedidoRepo.AdicionarAsync(pedido);

        try
        {
            // Um único SaveChanges grava pedido + baixa de estoque de forma atômica:
            // se algum item falhar, nada é persistido, e a baixa de estoque é
            // protegida por concorrência otimista (xmin) contra pedidos simultâneos.
            await _pedidoRepo.SalvarAlteracoesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflitoDeDadosException(
                "Um ou mais produtos foram alterados por outra operação ao mesmo tempo. Tente novamente.");
        }

        await _hub.Clients.Group("sellers")
            .SendAsync("NewOrder", new { orderId = pedido.Id, buyerName = comprador.Nome });

        var pedidoCompleto = await _pedidoRepo.ObterPorIdComDetalhesAsync(pedido.Id);
        return MapearParaDto(pedidoCompleto!);
    }

    public async Task<PagedPedidosDto> ListarAsync(
        string? role, int userId, string? perfilVendedor,
        string? status, DateTime? de, DateTime? ate,
        int pagina, int tamanhoPagina)
    {
        StatusPedido? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<StatusPedido>(status, out var s))
            statusEnum = s;

        pagina = pagina < 1 ? 1 : pagina;
        tamanhoPagina = tamanhoPagina is < 1 or > 100 ? 20 : tamanhoPagina;

        var compradorId = role == "Buyer" ? userId : (int?)null;
        var vendedorId = role == "Seller" ? userId : (int?)null;

        var (pedidos, totalCount) = await _pedidoRepo.ListarPaginadoAsync(
            compradorId, role, vendedorId, perfilVendedor, statusEnum, de, ate, pagina, tamanhoPagina);

        // Receita/lucro precisam refletir TODOS os pedidos que batem com o filtro,
        // não só a página atual — por isso é uma consulta agregada separada.
        var (totalValor, totalLucro) = await _pedidoRepo.ObterTotaisAsync(
            compradorId, role, vendedorId, perfilVendedor, statusEnum, de, ate);

        return new PagedPedidosDto(
            pedidos.Select(MapearParaDto),
            pagina, tamanhoPagina, totalCount,
            (int)Math.Ceiling(totalCount / (double)tamanhoPagina),
            totalValor, totalLucro);
    }

    public async Task<RespostaPedidoDto?> ObterPorIdAsync(int id)
    {
        var pedido = await _pedidoRepo.ObterPorIdComDetalhesAsync(id);
        return pedido is null ? null : MapearParaDto(pedido);
    }

    public async Task<ResultadoAtualizacaoStatusDto> AtualizarStatusAsync(int id, string status)
    {
        if (!Enum.TryParse<StatusPedido>(status, out var novoStatus))
            throw new DomainException("Status inválido.");

        var pedido = await _pedidoRepo.ObterPorIdComDetalhesAsync(id)
            ?? throw new DomainException("Pedido não encontrado.");

        pedido.AtualizarStatus(novoStatus);
        await _pedidoRepo.SalvarAlteracoesAsync();

        await _hub.Clients.Group($"buyer-{pedido.CompradorId}")
            .SendAsync("OrderStatusUpdated", new
            {
                orderId = pedido.Id,
                status,
                buyerName = pedido.Comprador.Nome
            });

        var urlWhatsApp = pedido.GerarLinkWhatsApp();
        return new ResultadoAtualizacaoStatusDto(status, urlWhatsApp);
    }

    public async Task<RespostaPedidoDto?> AtribuirVendedorAsync(int id, AtribuirVendedorDto dto)
    {
        var pedido = await _pedidoRepo.ObterPorIdComDetalhesAsync(id);
        if (pedido is null) return null;

        if (dto.AtendidoPorVendedorId.HasValue)
        {
            var vendedorValido = await _vendedorRepo.ExisteAprovadoPorIdAsync(dto.AtendidoPorVendedorId.Value);
            if (!vendedorValido)
                throw new DomainException("Vendedor informado não encontrado.");
        }

        pedido.AtribuirVendedor(dto.AtendidoPorVendedorId);
        await _pedidoRepo.SalvarAlteracoesAsync();

        var pedidoAtualizado = await _pedidoRepo.ObterPorIdComDetalhesAsync(id);
        return MapearParaDto(pedidoAtualizado!);
    }

    private static RespostaPedidoDto MapearParaDto(Pedido p) => new(
        p.Id, p.CompradorId,
        p.Comprador?.Nome ?? "", p.Comprador?.NomeLoja ?? "",
        p.Comprador?.Telefone ?? "",
        p.ValorTotal, p.LucroTotal,
        p.Status.ToString(), p.CriadoEm,
        p.AtendidoPorVendedorId, p.AtendidoPorVendedor?.Nome,
        p.Itens.Select(i => new ItemPedidoRespostaDto(
            i.ProdutoId, i.Produto?.Nome ?? "",
            i.Quantidade, i.PrecoUnitario, i.CustoUnitario, i.Lucro)).ToList());
}
