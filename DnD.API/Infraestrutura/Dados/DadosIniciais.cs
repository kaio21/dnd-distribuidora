using DnD.API.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Infraestrutura.Dados;

public static class DadosIniciais
{
    public static async Task PopularAsync(AppDbContext db)
    {
        if (await db.Vendedores.AnyAsync()) return;

        var vendedor = Vendedor.Criar(
            "Diego Silva", "123.456.789-00", "diego@dnd.com",
            BCrypt.Net.BCrypt.HashPassword("123456"), "Admin",
            DateTime.UtcNow.AddDays(-60));
        db.Vendedores.Add(vendedor);

        var c1 = Comprador.Criar(
            "João Ferreira", "Mercadinho do João", "joao@mercadinho.com",
            BCrypt.Net.BCrypt.HashPassword("123456"),
            "11987654321", "Rua das Flores, 123, Centro, São Paulo/SP",
            cpf: "987.654.321-00", cnpj: "12.345.678/0001-90",
            criadoEm: DateTime.UtcNow.AddDays(-45));

        var c2 = Comprador.Criar(
            "Maria Oliveira", "Loja da Maria", "maria@lojadamaria.com",
            BCrypt.Net.BCrypt.HashPassword("123456"),
            "11912345678", "Av. Paulista, 900, Bela Vista, São Paulo/SP",
            cpf: "321.654.987-11", cnpj: "98.765.432/0001-11",
            criadoEm: DateTime.UtcNow.AddDays(-30));

        var c3 = Comprador.Criar(
            "Carlos Mendes", "Atacado Mendes", "carlos@atacadomendes.com",
            BCrypt.Net.BCrypt.HashPassword("123456"),
            "11999887766", "Rua do Comércio, 450, Vila Industrial, Campinas/SP",
            cpf: "111.222.333-44", cnpj: "55.666.777/0001-88",
            criadoEm: DateTime.UtcNow.AddDays(-20));

        db.Compradores.AddRange(c1, c2, c3);

        var p1  = Produto.Criar("Camiseta Básica",     "Camiseta 100% algodão, tamanhos P ao XG, diversas cores.",       "Roupas",      18.00m, 35.00m,  120, DateTime.UtcNow.AddDays(-50));
        var p2  = Produto.Criar("Calça Jeans Slim",    "Calça jeans masculina slim fit, lavagens variadas.",             "Roupas",      45.00m, 89.90m,  75,  DateTime.UtcNow.AddDays(-48));
        var p3  = Produto.Criar("Tênis Casual",        "Tênis unissex leve e confortável, numeração 34 ao 44.",          "Calçados",    55.00m, 110.00m, 60,  DateTime.UtcNow.AddDays(-45));
        var p4  = Produto.Criar("Boné Aba Curva",      "Boné aba curva snapback, ajustável, diversas estampas.",         "Acessórios",  12.00m, 25.00m,  200, DateTime.UtcNow.AddDays(-42));
        var p5  = Produto.Criar("Meia Kit c/6 pares",  "Kit com 6 pares de meias cano médio, 100% algodão.",            "Acessórios",  10.00m, 22.00m,  300, DateTime.UtcNow.AddDays(-40));
        var p6  = Produto.Criar("Bermuda Moletom",     "Bermuda de moletom com bolso, tamanhos P ao XG.",               "Roupas",      22.00m, 45.00m,  90,  DateTime.UtcNow.AddDays(-38));
        var p7  = Produto.Criar("Vestido Floral",      "Vestido feminino floral midi, tecido leve para o verão.",        "Roupas",      38.00m, 75.00m,  55,  DateTime.UtcNow.AddDays(-35));
        var p8  = Produto.Criar("Jaqueta Corta-vento", "Jaqueta impermeável corta-vento, ideal para dias frios.",       "Roupas",      68.00m, 135.00m, 40,  DateTime.UtcNow.AddDays(-30));
        var p9  = Produto.Criar("Shorts Esportivo",    "Shorts esportivo dry-fit, ideal para academia e corrida.",      "Roupas",      16.00m, 32.00m,  0,   DateTime.UtcNow.AddDays(-28));
        var p10 = Produto.Criar("Cinto de Couro",      "Cinto masculino em couro legítimo, fivela dourada ou prata.",   "Acessórios",  14.00m, 29.90m,  150, DateTime.UtcNow.AddDays(-25));

        p9.AlternarAtividade(); // Shorts com estoque zero — desativar

        db.Produtos.AddRange(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
        await db.SaveChangesAsync();

        var pedidos = new List<Pedido>
        {
            CriarPedidoSeed(c1.Id, StatusPedido.Entregue, DateTime.UtcNow.AddDays(-30),
                (p1, 10), (p4, 20), (p5, 15)),

            CriarPedidoSeed(c1.Id, StatusPedido.Pronto, DateTime.UtcNow.AddDays(-5),
                (p2, 5), (p3, 8)),

            CriarPedidoSeed(c2.Id, StatusPedido.Entregue, DateTime.UtcNow.AddDays(-20),
                (p6, 12), (p7, 8), (p10, 25)),

            CriarPedidoSeed(c2.Id, StatusPedido.Confirmado, DateTime.UtcNow.AddDays(-2),
                (p8, 6)),

            CriarPedidoSeed(c3.Id, StatusPedido.Entregue, DateTime.UtcNow.AddDays(-15),
                (p1, 30), (p5, 40), (p4, 30)),

            CriarPedidoSeed(c3.Id, StatusPedido.Pendente, DateTime.UtcNow.AddHours(-2),
                (p2, 10), (p3, 10)),
        };

        db.Pedidos.AddRange(pedidos);

        var mensagens = new List<Mensagem>
        {
            CriarMensagemSeed(c1.Id, "Buyer", 1, "Seller", c1.Id, "Olá! Vocês têm camisetas no tam. GG disponíveis?",          lida: true,  criadoEm: DateTime.UtcNow.AddDays(-10).AddHours(-3)),
            CriarMensagemSeed(1,     "Seller", c1.Id, "Buyer", c1.Id, "Oi João! Sim, temos bastante estoque no GG. Posso separar?", lida: true,  criadoEm: DateTime.UtcNow.AddDays(-10).AddHours(-2)),
            CriarMensagemSeed(c1.Id, "Buyer", 1, "Seller", c1.Id, "Ótimo! Vou fazer o pedido agora mesmo.",                     lida: true,  criadoEm: DateTime.UtcNow.AddDays(-10).AddHours(-1)),
            CriarMensagemSeed(c1.Id, "Buyer", 1, "Seller", c1.Id, "Meu pedido já foi confirmado?",                              lida: false, criadoEm: DateTime.UtcNow.AddDays(-5).AddHours(-1)),

            CriarMensagemSeed(c2.Id, "Buyer", 1, "Seller", c2.Id, "Boa tarde! As jaquetas corta-vento têm previsão de mais estoque?", lida: true,  criadoEm: DateTime.UtcNow.AddDays(-3).AddHours(-5)),
            CriarMensagemSeed(1,     "Seller", c2.Id, "Buyer", c2.Id, "Boa tarde Maria! Temos 40 unidades disponíveis agora.",         lida: true,  criadoEm: DateTime.UtcNow.AddDays(-3).AddHours(-4)),
            CriarMensagemSeed(c2.Id, "Buyer", 1, "Seller", c2.Id, "Perfeito, vou pegar 6 unidades. Obrigada!",                        lida: false, criadoEm: DateTime.UtcNow.AddDays(-2).AddHours(-3)),

            CriarMensagemSeed(c3.Id, "Buyer", 1, "Seller", c3.Id, "Diego, vocês fazem entrega para Campinas?",                        lida: true,  criadoEm: DateTime.UtcNow.AddDays(-1).AddHours(-6)),
            CriarMensagemSeed(1,     "Seller", c3.Id, "Buyer", c3.Id, "Olá Carlos! Sim, entregamos. Frete calculado por região.",       lida: true,  criadoEm: DateTime.UtcNow.AddDays(-1).AddHours(-5)),
            CriarMensagemSeed(c3.Id, "Buyer", 1, "Seller", c3.Id, "Acabei de fazer um novo pedido, aguardo confirmação!",              lida: false, criadoEm: DateTime.UtcNow.AddHours(-1)),
        };

        db.Mensagens.AddRange(mensagens);
        await db.SaveChangesAsync();
    }

    private static Pedido CriarPedidoSeed(int compradorId, StatusPedido status, DateTime criadoEm, params (Produto produto, int qtd)[] itens)
    {
        var pedido = Pedido.Criar(compradorId);

        // Contorna a validação de estoque para seed (produtos já têm estoque ajustado)
        foreach (var (produto, qtd) in itens)
        {
            var item = ItemPedido.Criar(produto.Id, qtd, produto.PrecoVenda, produto.PrecoCusto);
            pedido.Itens.Add(item);
            pedido.AtualizarTotaisSeed(produto.PrecoVenda, produto.PrecoCusto, qtd);
        }

        pedido.AtualizarStatus(status);
        pedido.DefinirDataCriacaoSeed(criadoEm);
        return pedido;
    }

    private static Mensagem CriarMensagemSeed(
        int remetenteId, string tipoRemetente, int destinatarioId, string tipoDestinatario,
        int compradorId, string conteudo, bool lida, DateTime criadoEm)
    {
        var msg = Mensagem.Criar(remetenteId, tipoRemetente, destinatarioId, tipoDestinatario, conteudo);
        msg.DefinirCompradorIdSeed(compradorId);
        msg.DefinirDataSeed(criadoEm);
        if (lida) msg.MarcarComoLida();
        return msg;
    }
}
