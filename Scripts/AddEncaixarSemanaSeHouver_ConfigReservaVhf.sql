-- Migração: adicionar coluna EncaixarSemanaSeHouver na tabela ConfigReservaVhf.
-- Quando true: usar tarifa apenas como base de pesquisa e buscar uma que encaixe a semana da reserva.

ALTER TABLE portalohana."ConfigReservaVhf" ADD COLUMN IF NOT EXISTS "EncaixarSemanaSeHouver" BOOLEAN NOT NULL DEFAULT FALSE;

COMMENT ON COLUMN portalohana."ConfigReservaVhf"."EncaixarSemanaSeHouver" IS 'Usar tarifa como base de pesquisa e buscar uma que encaixe a semana da reserva';
