namespace DnD.API.Aplicacao.DTOs;

public record CriarVendedorDto(
    string Nome,
    string? CPF,
    string Email,
    string Senha,
    string Perfil
);

public record LoginVendedorDto(
    string Email,
    string Senha
);

public record RegistrarCompradorDto(
    string Nome,
    string NomeLoja,
    string Email,
    string Senha,
    string Telefone,
    string Endereco,
    string? CPF = null,
    string? CNPJ = null,
    int? IndicadoPorVendedorId = null
);

public record LoginCompradorDto(
    string Email,
    string Senha
);

public record RespostaAutenticacaoDto(
    string Token,
    string Role,
    int UsuarioId,
    string Nome,
    string Email,
    bool IsAdmin = false,
    string? PerfilVendedor = null
);
