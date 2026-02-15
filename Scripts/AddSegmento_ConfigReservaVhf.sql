-- Migração: adicionar coluna Segmento na tabela ConfigReservaVhf.
-- Segmento (CM): código do segmento de reserva utilizado nas reservas daquele tipo.

ALTER TABLE portalohana."ConfigReservaVhf" ADD COLUMN IF NOT EXISTS "Segmento" VARCHAR(100) NULL;

COMMENT ON COLUMN portalohana."ConfigReservaVhf"."Segmento" IS 'Código do segmento de reserva (CM)';
