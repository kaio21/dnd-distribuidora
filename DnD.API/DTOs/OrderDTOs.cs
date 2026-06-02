namespace DnD.API.DTOs;

public record OrderItemRequestDto(
    int ProductId,
    int Quantity
);

public record CreateOrderDto(
    List<OrderItemRequestDto> Items,
    int? AttendedBySellerId = null
);

public record AssignSellerDto(int? AttendedBySellerId);

public record OrderItemResponseDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal UnitCost,
    decimal Profit
);

public record OrderResponseDto(
    int Id,
    int BuyerId,
    string BuyerName,
    string BuyerStoreName,
    string BuyerPhone,
    decimal TotalAmount,
    decimal TotalProfit,
    string Status,
    DateTime CreatedAt,
    int? AttendedBySellerId,
    string? AttendedBySellerName,
    List<OrderItemResponseDto> Items
);

public record UpdateOrderStatusDto(string Status);
