-- Torna as colunas DataFimVigenciaCriacao e DataFimVigenciaUso opcionais (nullable)
-- Data fim de vigência de criação e utilização não são mais obrigatórias

ALTER TABLE portalohana."RegraIntercambio"
  ALTER COLUMN "DataFimVigenciaCriacao" DROP NOT NULL;

ALTER TABLE portalohana."RegraIntercambio"
  ALTER COLUMN "DataFimVigenciaUso" DROP NOT NULL;
