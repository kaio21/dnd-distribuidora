using DnD.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Sellers.AnyAsync()) return;

        // ── Vendedor ──────────────────────────────────────────────
        var seller = new Seller
        {
            Name     = "Diego Silva",
            CPF      = "123.456.789-00",
            Email    = "diego@dnd.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role      = "Admin",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };
        db.Sellers.Add(seller);

        // ── Compradores ───────────────────────────────────────────
        var b1 = new Buyer
        {
            Name      = "João Ferreira",
            StoreName = "Mercadinho do João",
            CPF       = "987.654.321-00",
            CNPJ      = "12.345.678/0001-90",
            Email     = "joao@mercadinho.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone     = "11987654321",
            Address   = "Rua das Flores, 123, Centro, São Paulo/SP",
            CreatedAt = DateTime.UtcNow.AddDays(-45)
        };
        var b2 = new Buyer
        {
            Name      = "Maria Oliveira",
            StoreName = "Loja da Maria",
            CPF       = "321.654.987-11",
            CNPJ      = "98.765.432/0001-11",
            Email     = "maria@lojadamaria.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone     = "11912345678",
            Address   = "Av. Paulista, 900, Bela Vista, São Paulo/SP",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        var b3 = new Buyer
        {
            Name      = "Carlos Mendes",
            StoreName = "Atacado Mendes",
            CPF       = "111.222.333-44",
            CNPJ      = "55.666.777/0001-88",
            Email     = "carlos@atacadomendes.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone     = "11999887766",
            Address   = "Rua do Comércio, 450, Vila Industrial, Campinas/SP",
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };
        db.Buyers.AddRange(b1, b2, b3);

        // ── Produtos ──────────────────────────────────────────────
        var p1 = new Product { Name = "Camiseta Básica",       Description = "Camiseta 100% algodão, tamanhos P ao XG, diversas cores.",         CostPrice = 18.00m,  SalePrice = 35.00m,  Stock = 120, IsActive = true,  CreatedAt = DateTime.UtcNow.AddDays(-50) };
        var p2 = new Product { Name = "Calça Jeans Slim",      Description = "Calça jeans masculina slim fit, lavagens variadas.",               CostPrice = 45.00m,  SalePrice = 89.90m,  Stock = 75,  IsActive = true,  CreatedAt = DateTime.UtcNow.AddDays(-48) };
        var p3 = new Product { Name = "Tênis Casual",          Description = "Tênis unissex leve e confortável, numeração 34 ao 44.",            CostPrice = 55.00m,  SalePrice = 110.00m, Stock = 60,  IsActive = true,  CreatedAt = DateTime.UtcNow.AddDays(-45) };
        var p4 = new Product { Name = "Boné Aba Curva",        Description = "Boné aba curva snapback, ajustável, diversas estampas.",           CostPrice = 12.00m,  SalePrice = 25.00m,  Stock = 200, IsActive = true,  CreatedAt = DateTime.UtcNow.AddDays(-42) };
        var p5 = new Product { Name = "Meia Kit c/6 pares",    Description = "Kit com 6 pares de meias cano médio, 100% algodão.",               CostPrice = 10.00m,  SalePrice = 22.00m,  Stock = 300, IsActive = true,  CreatedAt = DateTime.UtcNow.AddDays(-40) };
        var p6 = new Product { Name = "Bermuda Moletom",       Description = "Bermuda de moletom com bolso, tamanhos P ao XG.",                  CostPrice = 22.00m,  SalePrice = 45.00m,  Stock = 90,  IsActive = true,  CreatedAt = DateTime.UtcNow.AddDays(-38) };
        var p7 = new Product { Name = "Vestido Floral",        Description = "Vestido feminino floral midi, tecido leve para o verão.",          CostPrice = 38.00m,  SalePrice = 75.00m,  Stock = 55,  IsActive = true,  CreatedAt = DateTime.UtcNow.AddDays(-35) };
        var p8 = new Product { Name = "Jaqueta Corta-vento",   Description = "Jaqueta impermeável corta-vento, ideal para dias frios.",         CostPrice = 68.00m,  SalePrice = 135.00m, Stock = 40,  IsActive = true,  CreatedAt = DateTime.UtcNow.AddDays(-30) };
        var p9 = new Product { Name = "Shorts Esportivo",      Description = "Shorts esportivo dry-fit, ideal para academia e corrida.",        CostPrice = 16.00m,  SalePrice = 32.00m,  Stock = 0,   IsActive = false, CreatedAt = DateTime.UtcNow.AddDays(-28) };
        var p10= new Product { Name = "Cinto de Couro",        Description = "Cinto masculino em couro legítimo, fivela dourada ou prata.",     CostPrice = 14.00m,  SalePrice = 29.90m,  Stock = 150, IsActive = true,  CreatedAt = DateTime.UtcNow.AddDays(-25) };
        db.Products.AddRange(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);

        await db.SaveChangesAsync();

        // ── Pedidos ───────────────────────────────────────────────
        var orders = new List<Order>
        {
            // João — Pedido 1 (Entregue, há 30 dias)
            new Order
            {
                BuyerId     = b1.Id,
                Status      = OrderStatus.Entregue,
                CreatedAt   = DateTime.UtcNow.AddDays(-30),
                TotalAmount = (p1.SalePrice * 10) + (p4.SalePrice * 20) + (p5.SalePrice * 15),
                TotalProfit = ((p1.SalePrice - p1.CostPrice) * 10) + ((p4.SalePrice - p4.CostPrice) * 20) + ((p5.SalePrice - p5.CostPrice) * 15),
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = p1.Id, Quantity = 10, UnitPrice = p1.SalePrice, UnitCost = p1.CostPrice, Profit = (p1.SalePrice - p1.CostPrice) * 10 },
                    new OrderItem { ProductId = p4.Id, Quantity = 20, UnitPrice = p4.SalePrice, UnitCost = p4.CostPrice, Profit = (p4.SalePrice - p4.CostPrice) * 20 },
                    new OrderItem { ProductId = p5.Id, Quantity = 15, UnitPrice = p5.SalePrice, UnitCost = p5.CostPrice, Profit = (p5.SalePrice - p5.CostPrice) * 15 },
                }
            },
            // João — Pedido 2 (Pronto, há 5 dias)
            new Order
            {
                BuyerId     = b1.Id,
                Status      = OrderStatus.Pronto,
                CreatedAt   = DateTime.UtcNow.AddDays(-5),
                TotalAmount = (p2.SalePrice * 5) + (p3.SalePrice * 8),
                TotalProfit = ((p2.SalePrice - p2.CostPrice) * 5) + ((p3.SalePrice - p3.CostPrice) * 8),
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = p2.Id, Quantity = 5,  UnitPrice = p2.SalePrice, UnitCost = p2.CostPrice, Profit = (p2.SalePrice - p2.CostPrice) * 5 },
                    new OrderItem { ProductId = p3.Id, Quantity = 8,  UnitPrice = p3.SalePrice, UnitCost = p3.CostPrice, Profit = (p3.SalePrice - p3.CostPrice) * 8 },
                }
            },
            // Maria — Pedido 1 (Entregue, há 20 dias)
            new Order
            {
                BuyerId     = b2.Id,
                Status      = OrderStatus.Entregue,
                CreatedAt   = DateTime.UtcNow.AddDays(-20),
                TotalAmount = (p6.SalePrice * 12) + (p7.SalePrice * 8) + (p10.SalePrice * 25),
                TotalProfit = ((p6.SalePrice - p6.CostPrice) * 12) + ((p7.SalePrice - p7.CostPrice) * 8) + ((p10.SalePrice - p10.CostPrice) * 25),
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = p6.Id,  Quantity = 12, UnitPrice = p6.SalePrice,  UnitCost = p6.CostPrice,  Profit = (p6.SalePrice  - p6.CostPrice)  * 12 },
                    new OrderItem { ProductId = p7.Id,  Quantity = 8,  UnitPrice = p7.SalePrice,  UnitCost = p7.CostPrice,  Profit = (p7.SalePrice  - p7.CostPrice)  * 8  },
                    new OrderItem { ProductId = p10.Id, Quantity = 25, UnitPrice = p10.SalePrice, UnitCost = p10.CostPrice, Profit = (p10.SalePrice - p10.CostPrice) * 25 },
                }
            },
            // Maria — Pedido 2 (Confirmado, há 2 dias)
            new Order
            {
                BuyerId     = b2.Id,
                Status      = OrderStatus.Confirmado,
                CreatedAt   = DateTime.UtcNow.AddDays(-2),
                TotalAmount = (p8.SalePrice * 6),
                TotalProfit = ((p8.SalePrice - p8.CostPrice) * 6),
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = p8.Id, Quantity = 6, UnitPrice = p8.SalePrice, UnitCost = p8.CostPrice, Profit = (p8.SalePrice - p8.CostPrice) * 6 },
                }
            },
            // Carlos — Pedido 1 (Entregue, há 15 dias)
            new Order
            {
                BuyerId     = b3.Id,
                Status      = OrderStatus.Entregue,
                CreatedAt   = DateTime.UtcNow.AddDays(-15),
                TotalAmount = (p1.SalePrice * 30) + (p5.SalePrice * 40) + (p4.SalePrice * 30),
                TotalProfit = ((p1.SalePrice - p1.CostPrice) * 30) + ((p5.SalePrice - p5.CostPrice) * 40) + ((p4.SalePrice - p4.CostPrice) * 30),
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = p1.Id, Quantity = 30, UnitPrice = p1.SalePrice, UnitCost = p1.CostPrice, Profit = (p1.SalePrice - p1.CostPrice) * 30 },
                    new OrderItem { ProductId = p5.Id, Quantity = 40, UnitPrice = p5.SalePrice, UnitCost = p5.CostPrice, Profit = (p5.SalePrice - p5.CostPrice) * 40 },
                    new OrderItem { ProductId = p4.Id, Quantity = 30, UnitPrice = p4.SalePrice, UnitCost = p4.CostPrice, Profit = (p4.SalePrice - p4.CostPrice) * 30 },
                }
            },
            // Carlos — Pedido 2 (Pendente, hoje)
            new Order
            {
                BuyerId     = b3.Id,
                Status      = OrderStatus.Pendente,
                CreatedAt   = DateTime.UtcNow.AddHours(-2),
                TotalAmount = (p2.SalePrice * 10) + (p3.SalePrice * 10),
                TotalProfit = ((p2.SalePrice - p2.CostPrice) * 10) + ((p3.SalePrice - p3.CostPrice) * 10),
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = p2.Id, Quantity = 10, UnitPrice = p2.SalePrice, UnitCost = p2.CostPrice, Profit = (p2.SalePrice - p2.CostPrice) * 10 },
                    new OrderItem { ProductId = p3.Id, Quantity = 10, UnitPrice = p3.SalePrice, UnitCost = p3.CostPrice, Profit = (p3.SalePrice - p3.CostPrice) * 10 },
                }
            },
        };
        db.Orders.AddRange(orders);

        // ── Mensagens ─────────────────────────────────────────────
        var messages = new List<Message>
        {
            new Message { SenderId = b1.Id, SenderType = "Buyer", ReceiverId = 1, ReceiverType = "Seller", BuyerId = b1.Id, Content = "Olá! Vocês têm camisetas no tam. GG disponíveis?",          IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-10).AddHours(-3) },
            new Message { SenderId = 1,     SenderType = "Seller", ReceiverId = b1.Id, ReceiverType = "Buyer", BuyerId = b1.Id, Content = "Oi João! Sim, temos bastante estoque no GG. Posso separar?", IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-10).AddHours(-2) },
            new Message { SenderId = b1.Id, SenderType = "Buyer", ReceiverId = 1, ReceiverType = "Seller", BuyerId = b1.Id, Content = "Ótimo! Vou fazer o pedido agora mesmo.",                     IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-10).AddHours(-1) },
            new Message { SenderId = b1.Id, SenderType = "Buyer", ReceiverId = 1, ReceiverType = "Seller", BuyerId = b1.Id, Content = "Meu pedido já foi confirmado?",                              IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-5).AddHours(-1) },

            new Message { SenderId = b2.Id, SenderType = "Buyer", ReceiverId = 1, ReceiverType = "Seller", BuyerId = b2.Id, Content = "Boa tarde! As jaquetas corta-vento têm previsão de mais estoque?", IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-3).AddHours(-5) },
            new Message { SenderId = 1,     SenderType = "Seller", ReceiverId = b2.Id, ReceiverType = "Buyer", BuyerId = b2.Id, Content = "Boa tarde Maria! Temos 40 unidades disponíveis agora.",         IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-3).AddHours(-4) },
            new Message { SenderId = b2.Id, SenderType = "Buyer", ReceiverId = 1, ReceiverType = "Seller", BuyerId = b2.Id, Content = "Perfeito, vou pegar 6 unidades. Obrigada!",                        IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-2).AddHours(-3) },

            new Message { SenderId = b3.Id, SenderType = "Buyer", ReceiverId = 1, ReceiverType = "Seller", BuyerId = b3.Id, Content = "Diego, vocês fazem entrega para Campinas?",                        IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-6) },
            new Message { SenderId = 1,     SenderType = "Seller", ReceiverId = b3.Id, ReceiverType = "Buyer", BuyerId = b3.Id, Content = "Olá Carlos! Sim, entregamos. Frete calculado por região.",       IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-5) },
            new Message { SenderId = b3.Id, SenderType = "Buyer", ReceiverId = 1, ReceiverType = "Seller", BuyerId = b3.Id, Content = "Acabei de fazer um novo pedido, aguardo confirmação!",              IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-1) },
        };
        db.Messages.AddRange(messages);

        await db.SaveChangesAsync();
    }
}
