-- Adiciona coluna Role aos Sellers e migra IsAdmin
-- Execute no SQL Editor do Supabase

ALTER TABLE "Sellers" ADD COLUMN IF NOT EXISTS "Role" text NOT NULL DEFAULT 'Vendedor';

-- Promover quem já era admin
UPDATE "Sellers" SET "Role" = 'Admin' WHERE "IsAdmin" = true;
