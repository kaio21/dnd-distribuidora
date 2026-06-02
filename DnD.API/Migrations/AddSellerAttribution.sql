-- Afiliação de vendas ao vendedor (PostgreSQL)
-- Idempotente: pode rodar mais de uma vez sem erro.
-- Em banco já existente (EnsureCreated não altera tabelas prontas), rode este script.

ALTER TABLE "Orders" ADD COLUMN IF NOT EXISTS "AttendedBySellerId" integer NULL;

DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Orders_AttendedBySeller') THEN
    ALTER TABLE "Orders" ADD CONSTRAINT "FK_Orders_AttendedBySeller"
      FOREIGN KEY ("AttendedBySellerId") REFERENCES "Sellers"("Id") ON DELETE SET NULL;
  END IF;
END $$;

ALTER TABLE "Buyers" ADD COLUMN IF NOT EXISTS "ReferredBySellerId" integer NULL;

DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Buyers_ReferredBySeller') THEN
    ALTER TABLE "Buyers" ADD CONSTRAINT "FK_Buyers_ReferredBySeller"
      FOREIGN KEY ("ReferredBySellerId") REFERENCES "Sellers"("Id") ON DELETE SET NULL;
  END IF;
END $$;
