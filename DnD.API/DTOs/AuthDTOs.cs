namespace DnD.API.DTOs;

public record SellerRegisterDto(
    string Name,
    string CPF,
    string Email,
    string Password
);

public record SellerLoginDto(
    string Email,
    string Password
);

public record BuyerRegisterDto(
    string Name,
    string StoreName,
    string CPF,
    string CNPJ,
    string Email,
    string Password,
    string Phone,
    string Address
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
    string Email
);
