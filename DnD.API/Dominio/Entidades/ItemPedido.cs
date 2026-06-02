using System.ComponentModel.DataAnnotations.Schema;

namespace DnD.API.Dominio.Entidades;

[Table("OrderItems")]
public class ItemPedido
{
    public int Id { get; private set; }

    [Column("OrderId")]
    public int PedidoId { get; private set; }

    public Pedido Pedido { get; private set; } = null!;

    [Column("ProductId")]
    public int ProdutoId { get; private set; }

    public Produto Produto { get; private set; } = null!;

    [Column("Quantity")]
    public int Quantidade { get; private set; }

    [Column("UnitPrice")]
    public decimal PrecoUnitario { get; private set; }

    [Column("UnitCost")]
    public decimal CustoUnitario { get; private set; }

    [Column("Profit")]
    public decimal Lucro { get; private set; }

    private ItemPedido() { }

    public static ItemPedido Criar(int produtoId, int quantidade, decimal precoUnitario, decimal custoUnitario) =>
        new()
        {
            ProdutoId = produtoId,
            Quantidade = quantidade,
            PrecoUnitario = precoUnitario,
            CustoUnitario = custoUnitario,
            Lucro = (precoUnitario - custoUnitario) * quantidade
        };
}
