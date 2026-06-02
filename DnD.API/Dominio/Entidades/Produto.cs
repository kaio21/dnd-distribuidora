using System.ComponentModel.DataAnnotations.Schema;
using DnD.API.Dominio;

namespace DnD.API.Dominio.Entidades;

[Table("Products")]
public class Produto
{
    public int Id { get; private set; }

    [Column("Name")]
    public string Nome { get; private set; } = string.Empty;

    [Column("Description")]
    public string Descricao { get; private set; } = string.Empty;

    [Column("Category")]
    public string NomeCategoria { get; private set; } = string.Empty;

    [Column("CostPrice")]
    public decimal PrecoCusto { get; private set; }

    [Column("SalePrice")]
    public decimal PrecoVenda { get; private set; }

    [Column("Stock")]
    public int Estoque { get; private set; }

    [Column("ImageUrl")]
    public string? UrlImagem { get; private set; }

    [Column("IsActive")]
    public bool Ativo { get; private set; } = true;

    [Column("CreatedAt")]
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    public ICollection<ItemPedido> ItensPedido { get; private set; } = new List<ItemPedido>();

    [NotMapped]
    public decimal Lucro => PrecoVenda - PrecoCusto;

    private Produto() { }

    public static Produto Criar(
        string nome, string descricao, string nomeCategoria,
        decimal precoCusto, decimal precoVenda, int estoque,
        DateTime? criadoEm = null) =>
        new()
        {
            Nome = nome,
            Descricao = descricao,
            NomeCategoria = nomeCategoria,
            PrecoCusto = precoCusto,
            PrecoVenda = precoVenda,
            Estoque = estoque,
            Ativo = true,
            CriadoEm = criadoEm ?? DateTime.UtcNow
        };

    public void Atualizar(string nome, string descricao, string nomeCategoria,
        decimal precoCusto, decimal precoVenda, int estoque, bool ativo)
    {
        Nome = nome;
        Descricao = descricao;
        NomeCategoria = nomeCategoria;
        PrecoCusto = precoCusto;
        PrecoVenda = precoVenda;
        Estoque = estoque;
        Ativo = ativo;
    }

    public void DefinirImagem(string? urlImagem) => UrlImagem = urlImagem;

    public void RemoverImagem() => UrlImagem = null;

    public void AlternarAtividade() => Ativo = !Ativo;

    public void ReducirEstoque(int quantidade)
    {
        if (Estoque < quantidade)
            throw new DomainException($"Estoque insuficiente para '{Nome}'. Disponível: {Estoque}");
        Estoque -= quantidade;
    }
}
