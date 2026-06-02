namespace DnD.API.DTOs;

public record ProductCreateDto(
    string Name,
    string Description,
    string Category,
    decimal CostPrice,
    decimal SalePrice,
    int Stock
);

public record ProductUpdateDto(
    string Name,
    string Description,
    string Category,
    decimal CostPrice,
    decimal SalePrice,
    int Stock,
    bool IsActive,
    bool RemoveImage = false
);

public record ProductResponseDto(
    int Id,
    string Name,
    string Description,
    string Category,
    decimal CostPrice,
    decimal SalePrice,
    decimal Profit,
    int Stock,
    string? ImageUrl,
    bool IsActive,
    DateTime CreatedAt
);
