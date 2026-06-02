namespace DnD.API.Aplicacao.DTOs;

public record ResumoDashboardDto(
    decimal ReceitaTotal,
    decimal LucroTotal,
    int TotalPedidos,
    int TotalProdutos,
    int ProdutosAtivos
);

public record PeriodoVendasDto(
    string Rotulo,
    decimal Receita,
    decimal Lucro,
    int Pedidos
);

public record RespostaDashboardDto(
    ResumoDashboardDto Resumo,
    List<PeriodoVendasDto> DadosDiarios,
    List<PeriodoVendasDto> DadosSemanais,
    List<TopProdutoDto> TopProdutos
);

public record TopProdutoDto(
    string NomeProduto,
    int QuantidadeVendida,
    decimal Receita,
    decimal Lucro
);
