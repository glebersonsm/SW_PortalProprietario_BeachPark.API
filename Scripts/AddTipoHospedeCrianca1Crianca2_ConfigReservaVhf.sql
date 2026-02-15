-- Migração: adicionar colunas TipoHospedeCrianca1 e TipoHospedeCrianca2 na tabela ConfigReservaVhf.
-- Tipo de hóspede para Criança1 (6-11 anos) e Criança2 (0-5 anos). Opcionais.

ALTER TABLE portalohana."ConfigReservaVhf" ADD COLUMN IF NOT EXISTS "TipoHospedeCrianca1" VARCHAR(100) NULL;
ALTER TABLE portalohana."ConfigReservaVhf" ADD COLUMN IF NOT EXISTS "TipoHospedeCrianca2" VARCHAR(100) NULL;

COMMENT ON COLUMN portalohana."ConfigReservaVhf"."TipoHospedeCrianca1" IS 'Tipo de hóspede para Criança1 (5-12 anos). Opcional.';
COMMENT ON COLUMN portalohana."ConfigReservaVhf"."TipoHospedeCrianca2" IS 'Tipo de hóspede para Criança2 (0-4 anos). Opcional.';
