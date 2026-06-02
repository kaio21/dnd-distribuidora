using System.Text.Json.Serialization;

namespace DnD.API.Aplicacao.DTOs;

public record CriarProdutoDto(
    [property: JsonPropertyName("name")]        string Nome,
    [property: JsonPropertyName("description")] string Descricao,
    [property: JsonPropertyName("category")]    string NomeCategoria,
    [property: JsonPropertyName("costPrice")]   decimal PrecoCusto,
    [property: JsonPropertyName("salePrice")]   decimal PrecoVenda,
    [property: JsonPropertyName("stock")]       int Estoque
);

public record AtualizarProdutoDto(
    [property: JsonPropertyName("name")]        string Nome,
    [property: JsonPropertyName("description")] string Descricao,
    [property: JsonPropertyName("category")]    string NomeCategoria,
    [property: JsonPropertyName("costPrice")]   decimal PrecoCusto,
    [property: JsonPropertyName("salePrice")]   decimal PrecoVenda,
    [property: JsonPropertyName("stock")]       int Estoque,
    [property: JsonPropertyName("isActive")]    bool Ativo,
    [property: JsonPropertyName("removeImage")] bool RemoverImagem = false
);

public record RespostaProdutoDto(
    [property: JsonPropertyName("id")]          int Id,
    [property: JsonPropertyName("name")]        string Nome,
    [property: JsonPropertyName("description")] string Descricao,
    [property: JsonPropertyName("category")]    string NomeCategoria,
    [property: JsonPropertyName("costPrice")]   decimal PrecoCusto,
    [property: JsonPropertyName("salePrice")]   decimal PrecoVenda,
    [property: JsonPropertyName("profit")]      decimal Lucro,
    [property: JsonPropertyName("stock")]       int Estoque,
    [property: JsonPropertyName("imageUrl")]    string? UrlImagem,
    [property: JsonPropertyName("isActive")]    bool Ativo,
    [property: JsonPropertyName("createdAt")]   DateTime CriadoEm
);
