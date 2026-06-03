using System.Text.Json.Serialization;

namespace DnD.API.Aplicacao.DTOs;

// Nomes em inglês para coincidir com os campos do FormData enviados pelo frontend.
// JsonPropertyName não afeta [FromForm] — apenas JSON body.
public record CriarProdutoDto(
    string Name,
    string Description,
    string Category,
    decimal CostPrice,
    decimal SalePrice,
    int Stock
);

public record AtualizarProdutoDto(
    string Name,
    string Description,
    string Category,
    decimal CostPrice,
    decimal SalePrice,
    int Stock,
    bool IsActive,
    bool RemoveImage = false
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
