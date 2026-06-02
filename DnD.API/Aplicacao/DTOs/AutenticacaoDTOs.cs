using System.Text.Json.Serialization;

namespace DnD.API.Aplicacao.DTOs;

public record CriarVendedorDto(
    [property: JsonPropertyName("name")]     string Nome,
    [property: JsonPropertyName("cpf")]      string? CPF,
    [property: JsonPropertyName("email")]    string Email,
    [property: JsonPropertyName("password")] string Senha,
    [property: JsonPropertyName("role")]     string Perfil
);

public record LoginVendedorDto(
    [property: JsonPropertyName("email")]    string Email,
    [property: JsonPropertyName("password")] string Senha
);

public record RegistrarCompradorDto(
    [property: JsonPropertyName("name")]               string Nome,
    [property: JsonPropertyName("storeName")]          string NomeLoja,
    [property: JsonPropertyName("email")]              string Email,
    [property: JsonPropertyName("password")]           string Senha,
    [property: JsonPropertyName("phone")]              string Telefone,
    [property: JsonPropertyName("address")]            string Endereco,
    [property: JsonPropertyName("cpf")]                string? CPF = null,
    [property: JsonPropertyName("cnpj")]               string? CNPJ = null,
    [property: JsonPropertyName("referredBySellerId")] int? IndicadoPorVendedorId = null
);

public record LoginCompradorDto(
    [property: JsonPropertyName("email")]    string Email,
    [property: JsonPropertyName("password")] string Senha
);

public record RespostaAutenticacaoDto(
    [property: JsonPropertyName("token")]      string Token,
    [property: JsonPropertyName("role")]       string Role,
    [property: JsonPropertyName("userId")]     int UsuarioId,
    [property: JsonPropertyName("name")]       string Nome,
    [property: JsonPropertyName("email")]      string Email,
    [property: JsonPropertyName("isAdmin")]    bool IsAdmin = false,
    [property: JsonPropertyName("sellerRole")] string? PerfilVendedor = null
);
