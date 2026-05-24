namespace DnD.API.DTOs;

public record DashboardSummaryDto(
    decimal TotalRevenue,
    decimal TotalProfit,
    int TotalOrders,
    int TotalProducts,
    int ActiveProducts
);

public record SalesPeriodDto(
    string Label,
    decimal Revenue,
    decimal Profit,
    int Orders
);

public record DashboardResponseDto(
    DashboardSummaryDto Summary,
    List<SalesPeriodDto> DailyData,
    List<SalesPeriodDto> WeeklyData,
    List<TopProductDto> TopProducts
);

public record TopProductDto(
    string ProductName,
    int QuantitySold,
    decimal Revenue,
    decimal Profit
);
