namespace DnD.API.Aplicacao.DTOs;

public record CriarProdutoDto(
    string Nome,
    string Descricao,
    string NomeCategoria,
    decimal PrecoCusto,
    decimal PrecoVenda,
    int Estoque
);

public record AtualizarProdutoDto(
    string Nome,
    string Descricao,
    string NomeCategoria,
    decimal PrecoCusto,
    decimal PrecoVenda,
    int Estoque,
    bool Ativo,
    bool RemoverImagem = false
);

public record RespostaProdutoDto(
    int Id,
    string Nome,
    string Descricao,
    string NomeCategoria,
    decimal PrecoCusto,
    decimal PrecoVenda,
    decimal Lucro,
    int Estoque,
    string? UrlImagem,
    bool Ativo,
    DateTime CriadoEm
);
