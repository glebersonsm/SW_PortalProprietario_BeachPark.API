-- Adiciona coluna TiposUhIds para permitir filtrar regras de intercâmbio por tipos de UH (TipoUh do eSolution)
-- Valores: IDs separados por vírgula (ex: 1,2,3). NULL ou vazio = aplica a todos os tipos.

ALTER TABLE portalohana."RegraIntercambio"
  ADD COLUMN IF NOT EXISTS "TiposUhIds" VARCHAR(500) NULL;

COMMENT ON COLUMN portalohana."RegraIntercambio"."TiposUhIds" IS 'IDs dos tipos de UH (TipoUh) permitidos, separados por vírgula. NULL/vazio = todos os tipos.';
