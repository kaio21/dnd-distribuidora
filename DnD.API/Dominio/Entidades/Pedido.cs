using System.ComponentModel.DataAnnotations.Schema;
using DnD.API.Dominio;

namespace DnD.API.Dominio.Entidades;

public enum StatusPedido
{
    Pendente,
    Confirmado,
    Pronto,
    Entregue,
    Cancelado
}

[Table("Orders")]
public class Pedido
{
    public int Id { get; private set; }

    [Column("BuyerId")]
    public int CompradorId { get; private set; }

    public Comprador Comprador { get; private set; } = null!;

    [Column("TotalAmount")]
    public decimal ValorTotal { get; private set; }

    [Column("TotalProfit")]
    public decimal LucroTotal { get; private set; }

    public StatusPedido Status { get; private set; } = StatusPedido.Pendente;

    [Column("CreatedAt")]
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    [Column("AttendedBySellerId")]
    public int? AtendidoPorVendedorId { get; private set; }

    public Vendedor? AtendidoPorVendedor { get; private set; }

    public ICollection<ItemPedido> Itens { get; private set; } = new List<ItemPedido>();

    private Pedido() { }

    public static Pedido Criar(int compradorId, int? atendidoPorVendedorId = null) =>
        new()
        {
            CompradorId = compradorId,
            AtendidoPorVendedorId = atendidoPorVendedorId,
            CriadoEm = DateTime.UtcNow
        };

    public void AdicionarItem(Produto produto, int quantidade)
    {
        if (!produto.Ativo)
            throw new DomainException($"Produto '{produto.Nome}' está pausado.");

        if (produto.Estoque < quantidade)
            throw new DomainException($"Estoque insuficiente para '{produto.Nome}'. Disponível: {produto.Estoque}");

        var item = ItemPedido.Criar(produto.Id, quantidade, produto.PrecoVenda, produto.PrecoCusto);
        Itens.Add(item);

        ValorTotal += produto.PrecoVenda * quantidade;
        LucroTotal += (produto.PrecoVenda - produto.PrecoCusto) * quantidade;

        produto.ReducirEstoque(quantidade);
    }

    public void AtualizarStatus(StatusPedido novoStatus) => Status = novoStatus;

    public void AtribuirVendedor(int? vendedorId) => AtendidoPorVendedorId = vendedorId;

    // Métodos internos exclusivos para seed de dados (não usam regras de negócio)
    internal void AdicionarItemSeed(ItemPedido item, decimal precoVenda, decimal precoCusto, int quantidade)
    {
        Itens.Add(item);
        ValorTotal += precoVenda * quantidade;
        LucroTotal += (precoCusto == 0 ? 0 : (precoVenda - precoCusto)) * quantidade;
    }

    internal void AtualizarTotaisSeed(decimal precoVenda, decimal precoCusto, int quantidade)
    {
        ValorTotal += precoVenda * quantidade;
        LucroTotal += (precoVenda - precoCusto) * quantidade;
    }

    internal void DefinirDataCriacaoSeed(DateTime data) => CriadoEm = data;

    public string? GerarLinkWhatsApp()
    {
        if (Status != StatusPedido.Pronto || Comprador == null)
            return null;

        var telefone = Comprador.Telefone
            .Replace("+", "").Replace("-", "").Replace(" ", "")
            .Replace("(", "").Replace(")", "");

        var mensagem = Uri.EscapeDataString(
            $"Olá {Comprador.Nome}! Seu pedido #{Id} está PRONTO para retirada/entrega. Total: R$ {ValorTotal:F2}");

        return $"https://wa.me/{telefone}?text={mensagem}";
    }
}
