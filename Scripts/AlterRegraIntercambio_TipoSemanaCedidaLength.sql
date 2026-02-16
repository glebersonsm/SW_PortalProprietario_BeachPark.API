-- Aumenta tamanho da coluna TipoSemanaCedida para suportar múltiplos valores (separados por vírgula).
-- Vazio = aplica a todos os tipos de semana cedida.

ALTER TABLE portalohana."RegraIntercambio"
  ALTER COLUMN "TipoSemanaCedida" TYPE VARCHAR(500);

COMMENT ON COLUMN portalohana."RegraIntercambio"."TipoSemanaCedida" IS 'Tipos de semana cedida (eSolution), separados por vírgula. Vazio = todos os tipos.';
