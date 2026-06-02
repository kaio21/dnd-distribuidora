using System.Text.Json.Serialization;

namespace DnD.API.Aplicacao.DTOs;

public record ItemPedidoRequisicaoDto(
    [property: JsonPropertyName("productId")] int ProdutoId,
    [property: JsonPropertyName("quantity")]  int Quantidade
);

public record CriarPedidoDto(
    [property: JsonPropertyName("items")]               List<ItemPedidoRequisicaoDto> Itens,
    [property: JsonPropertyName("attendedBySellerId")]  int? AtendidoPorVendedorId = null
);

public record AtribuirVendedorDto(
    [property: JsonPropertyName("attendedBySellerId")] int? AtendidoPorVendedorId
);

public record AtualizarStatusPedidoDto(
    [property: JsonPropertyName("status")] string Status
);

public record ItemPedidoRespostaDto(
    [property: JsonPropertyName("productId")]   int ProdutoId,
    [property: JsonPropertyName("productName")] string NomeProduto,
    [property: JsonPropertyName("quantity")]    int Quantidade,
    [property: JsonPropertyName("unitPrice")]   decimal PrecoUnitario,
    [property: JsonPropertyName("unitCost")]    decimal CustoUnitario,
    [property: JsonPropertyName("profit")]      decimal Lucro
);

public record RespostaPedidoDto(
    [property: JsonPropertyName("id")]                   int Id,
    [property: JsonPropertyName("buyerId")]              int CompradorId,
    [property: JsonPropertyName("buyerName")]            string NomeComprador,
    [property: JsonPropertyName("buyerStoreName")]       string NomeLoja,
    [property: JsonPropertyName("buyerPhone")]           string TelefoneComprador,
    [property: JsonPropertyName("totalAmount")]          decimal ValorTotal,
    [property: JsonPropertyName("totalProfit")]          decimal LucroTotal,
    [property: JsonPropertyName("status")]               string Status,
    [property: JsonPropertyName("createdAt")]            DateTime CriadoEm,
    [property: JsonPropertyName("attendedBySellerId")]   int? AtendidoPorVendedorId,
    [property: JsonPropertyName("attendedBySellerName")] string? NomeVendedorAtendente,
    [property: JsonPropertyName("items")]                List<ItemPedidoRespostaDto> Itens
);

public record ResultadoAtualizacaoStatusDto(
    [property: JsonPropertyName("status")]      string Status,
    [property: JsonPropertyName("whatsappUrl")] string? UrlWhatsApp
);
