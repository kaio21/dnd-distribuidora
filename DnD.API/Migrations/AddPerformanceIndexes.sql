-- Índices de performance para as consultas mais frequentes (listagem/paginação
-- de pedidos, filtro de categoria de produtos, agregação de conversas do chat).
-- O app usa EnsureCreated (não EF Migrations), então isso precisa ser rodado
-- manualmente no banco de produção — execute no SQL Editor do Supabase.

CREATE INDEX IF NOT EXISTS "IX_Orders_CreatedAt" ON "Orders" ("CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_Products_Category" ON "Products" ("Category");
CREATE INDEX IF NOT EXISTS "IX_Messages_CreatedAt" ON "Messages" ("CreatedAt");
