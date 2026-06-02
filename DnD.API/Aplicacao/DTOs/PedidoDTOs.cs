namespace DnD.API.Aplicacao.DTOs;

public record ItemPedidoRequisicaoDto(
    int ProdutoId,
    int Quantidade
);

public record CriarPedidoDto(
    List<ItemPedidoRequisicaoDto> Itens,
    int? AtendidoPorVendedorId = null
);

public record AtribuirVendedorDto(int? AtendidoPorVendedorId);

public record AtualizarStatusPedidoDto(string Status);

public record ItemPedidoRespostaDto(
    int ProdutoId,
    string NomeProduto,
    int Quantidade,
    decimal PrecoUnitario,
    decimal CustoUnitario,
    decimal Lucro
);

public record RespostaPedidoDto(
    int Id,
    int CompradorId,
    string NomeComprador,
    string NomeLoja,
    string TelefoneComprador,
    decimal ValorTotal,
    decimal LucroTotal,
    string Status,
    DateTime CriadoEm,
    int? AtendidoPorVendedorId,
    string? NomeVendedorAtendente,
    List<ItemPedidoRespostaDto> Itens
);

public record ResultadoAtualizacaoStatusDto(string Status, string? UrlWhatsApp);
