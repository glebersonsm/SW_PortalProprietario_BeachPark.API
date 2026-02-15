-- Migração: adicionar coluna TipoNegocio na tabela ConfigReservaVhf.
-- Opções: Não importa, Timesharing, Multipropriedade.

ALTER TABLE portalohana."ConfigReservaVhf" ADD COLUMN IF NOT EXISTS "TipoNegocio" VARCHAR(100) NULL;

COMMENT ON COLUMN portalohana."ConfigReservaVhf"."TipoNegocio" IS 'Tipo de negócio: Não importa, Timesharing ou Multipropriedade';
