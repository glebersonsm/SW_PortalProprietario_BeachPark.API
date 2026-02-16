-- Migração: criar tabela RegraIntercambio para regras de intercâmbio com vigência.
-- Permite configurar: tipo de contrato (ou todos), semana cedida, semanas permitidas para uso, períodos de vigência.

CREATE TABLE IF NOT EXISTS portalohana."RegraIntercambio" (
    "Id" SERIAL PRIMARY KEY,
    "ObjectGuid" VARCHAR(100) NULL,
    "UsuarioCriacao" INT NULL,
    "DataHoraCriacao" TIMESTAMP NULL,
    "DataHoraAlteracao" TIMESTAMP NULL,
    "UsuarioAlteracao" INT NULL,
    "TipoContratoId" INT NULL,
    "TipoSemanaCedida" VARCHAR(50) NOT NULL,
    "TiposSemanaPermitidosUso" VARCHAR(500) NOT NULL,
    "DataInicioVigenciaCriacao" DATE NOT NULL,
    "DataFimVigenciaCriacao" DATE NOT NULL,
    "DataInicioVigenciaUso" DATE NOT NULL,
    "DataFimVigenciaUso" DATE NOT NULL
);

COMMENT ON TABLE portalohana."RegraIntercambio" IS 'Regras de intercâmbio: define quais tipos de semana podem ser utilizados ao ceder um tipo específico, com períodos de vigência para criação e uso da reserva.';
COMMENT ON COLUMN portalohana."RegraIntercambio"."TipoContratoId" IS 'ID do tipo de contrato (eSolution). NULL = aplica a todos os contratos.';
COMMENT ON COLUMN portalohana."RegraIntercambio"."TipoSemanaCedida" IS 'Tipo de semana que está sendo cedida (ex: Super Alta, Alta, Média, Baixa).';
COMMENT ON COLUMN portalohana."RegraIntercambio"."TiposSemanaPermitidosUso" IS 'Tipos de semana permitidos para uso, separados por vírgula (ex: Super Alta,Média,Baixa).';
COMMENT ON COLUMN portalohana."RegraIntercambio"."DataInicioVigenciaCriacao" IS 'Início da vigência para criação da reserva.';
COMMENT ON COLUMN portalohana."RegraIntercambio"."DataFimVigenciaCriacao" IS 'Fim da vigência para criação da reserva.';
COMMENT ON COLUMN portalohana."RegraIntercambio"."DataInicioVigenciaUso" IS 'Início da vigência para utilização da reserva.';
COMMENT ON COLUMN portalohana."RegraIntercambio"."DataFimVigenciaUso" IS 'Fim da vigência para utilização da reserva.';
