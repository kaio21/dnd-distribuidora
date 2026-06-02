namespace DnD.API.DTOs;

public record CreateSellerDto(
    string Name,
    string? CPF,
    string Email,
    string Password,
    string Role // "Vendedor" ou "Gerente"
);

public record SellerLoginDto(
    string Email,
    string Password
);

public record BuyerRegisterDto(
    string Name,
    string StoreName,
    string Email,
    string Password,
    string Phone,
    string Address,
    string? CPF = null,
    string? CNPJ = null,
    int? ReferredBySellerId = null
);

public record BuyerLoginDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    string Token,
    string Role,
    int UserId,
    string Name,
    string Email,
    bool IsAdmin = false,
    string? SellerRole = null
);
