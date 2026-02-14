-- Migração: adicionar coluna PermiteIntercambioMultipropriedade na tabela ConfigReservaVhf.
-- Indica se o hotel permite intercâmbio com Multipropriedade.
-- PostgreSQL (schema portalohana)

ALTER TABLE portalohana."ConfigReservaVhf" ADD COLUMN IF NOT EXISTS "PermiteIntercambioMultipropriedade" BOOLEAN DEFAULT FALSE NOT NULL;

COMMENT ON COLUMN portalohana."ConfigReservaVhf"."PermiteIntercambioMultipropriedade" IS 'Indica se o hotel permite intercâmbio com Multipropriedade';
