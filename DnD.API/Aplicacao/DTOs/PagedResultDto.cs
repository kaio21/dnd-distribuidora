namespace DnD.API.Aplicacao.DTOs;

public record PagedResultDto<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
