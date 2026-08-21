namespace DnD.API.Aplicacao.DTOs;

public record PagedPedidosDto(
    IEnumerable<RespostaPedidoDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    decimal TotalRevenue,
    decimal TotalProfit);
