-- Tabela para configurações padrão de reservas VHF (PMS).
-- Armazena valores padrão para integração com sistemas legados.
-- Execute no banco de dados. Ajuste o schema conforme o ambiente (portalohana para PostgreSQL).

-- PostgreSQL (schema portalohana)
CREATE TABLE IF NOT EXISTS portalohana."ConfigReservaVhf" (
    "Id" SERIAL PRIMARY KEY,
    "ObjectGuid" VARCHAR(100) NULL,
    "UsuarioCriacao" INT NULL,
    "DataHoraCriacao" TIMESTAMP NULL,
    "DataHoraAlteracao" TIMESTAMP NULL,
    "UsuarioAlteracao" INT NULL,
    "TipoUtilizacao" VARCHAR(100) NOT NULL,
    "HotelId" INT NULL,
    "TipoHospede" VARCHAR(100) NOT NULL,
    "Origem" VARCHAR(100) NOT NULL,
    "TarifaHotel" VARCHAR(100) NOT NULL,
    "CodigoPensao" VARCHAR(100) NOT NULL,
    "PermiteIntercambioMultipropriedade" BOOLEAN DEFAULT FALSE NOT NULL
);

-- Sequência para o gerador Native do NHibernate (ConfigReservaVhf_)
CREATE SEQUENCE IF NOT EXISTS portalohana."ConfigReservaVhf_" START 1;

-- Comentários
COMMENT ON TABLE portalohana."ConfigReservaVhf" IS 'Configurações padrão para reservas VHF (PMS)';
COMMENT ON COLUMN portalohana."ConfigReservaVhf"."TipoUtilizacao" IS 'Uso próprio ou Uso convidado';
COMMENT ON COLUMN portalohana."ConfigReservaVhf"."HotelId" IS 'ID da unidade (Empresa)';
COMMENT ON COLUMN portalohana."ConfigReservaVhf"."TipoHospede" IS 'Categoria padrão (ex: Lazer, Corporativo)';
COMMENT ON COLUMN portalohana."ConfigReservaVhf"."Origem" IS 'Canal de venda padrão (ex: Direto, Motor de Reservas)';
COMMENT ON COLUMN portalohana."ConfigReservaVhf"."TarifaHotel" IS 'Código da tarifa base (ex: BAR, Acordo)';
COMMENT ON COLUMN portalohana."ConfigReservaVhf"."CodigoPensao" IS 'Regime de alimentação (ex: SO, BB, MAP)';
COMMENT ON COLUMN portalohana."ConfigReservaVhf"."PermiteIntercambioMultipropriedade" IS 'Indica se o hotel permite intercâmbio com Multipropriedade';
