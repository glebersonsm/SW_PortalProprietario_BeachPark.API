-- Migra TipoContratoId (único) para TipoContratoIds (múltiplos, separados por vírgula).
-- Vazio = aplica a todos os tipos de contrato.

-- 1. Adicionar nova coluna
ALTER TABLE portalohana."RegraIntercambio"
  ADD COLUMN IF NOT EXISTS "TipoContratoIds" VARCHAR(500) NULL;

-- 2. Migrar dados existentes
UPDATE portalohana."RegraIntercambio"
  SET "TipoContratoIds" = CAST("TipoContratoId" AS VARCHAR)
  WHERE "TipoContratoId" IS NOT NULL;

UPDATE portalohana."RegraIntercambio"
  SET "TipoContratoIds" = ''
  WHERE "TipoContratoId" IS NULL;

-- 3. Remover coluna antiga
ALTER TABLE portalohana."RegraIntercambio"
  DROP COLUMN IF EXISTS "TipoContratoId";

COMMENT ON COLUMN portalohana."RegraIntercambio"."TipoContratoIds" IS 'IDs dos tipos de contrato (eSolution), separados por vírgula. NULL/vazio = todos.';
