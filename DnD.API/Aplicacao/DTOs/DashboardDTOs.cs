using System.Text.Json.Serialization;

namespace DnD.API.Aplicacao.DTOs;

public record ResumoDashboardDto(
    [property: JsonPropertyName("totalRevenue")]   decimal ReceitaTotal,
    [property: JsonPropertyName("totalProfit")]    decimal LucroTotal,
    [property: JsonPropertyName("totalOrders")]    int TotalPedidos,
    [property: JsonPropertyName("totalProducts")]  int TotalProdutos,
    [property: JsonPropertyName("activeProducts")] int ProdutosAtivos
);

public record PeriodoVendasDto(
    [property: JsonPropertyName("label")]   string Rotulo,
    [property: JsonPropertyName("revenue")] decimal Receita,
    [property: JsonPropertyName("profit")]  decimal Lucro,
    [property: JsonPropertyName("orders")]  int Pedidos
);

public record RespostaDashboardDto(
    [property: JsonPropertyName("summary")]     ResumoDashboardDto Resumo,
    [property: JsonPropertyName("dailyData")]   List<PeriodoVendasDto> DadosDiarios,
    [property: JsonPropertyName("weeklyData")]  List<PeriodoVendasDto> DadosSemanais,
    [property: JsonPropertyName("topProducts")] List<TopProdutoDto> TopProdutos
);

public record TopProdutoDto(
    [property: JsonPropertyName("productName")]    string NomeProduto,
    [property: JsonPropertyName("quantitySold")]   int QuantidadeVendida,
    [property: JsonPropertyName("revenue")]        decimal Receita,
    [property: JsonPropertyName("profit")]         decimal Lucro
);
