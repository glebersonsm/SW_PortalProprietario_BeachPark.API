-- Migração: adicionar colunas de configuração 2FA em ParametroSistema
-- Execute conforme o banco em uso (SQL Server). Ajuste tipo se o projeto usar outro (ex.: bit para SQL Server).

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParametroSistema') AND name = 'Habilitar2FAPorEmail')
BEGIN
    ALTER TABLE ParametroSistema ADD Habilitar2FAPorEmail INT NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParametroSistema') AND name = 'Habilitar2FAPorSms')
BEGIN
    ALTER TABLE ParametroSistema ADD Habilitar2FAPorSms INT NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParametroSistema') AND name = 'Habilitar2FAParaCliente')
BEGIN
    ALTER TABLE ParametroSistema ADD Habilitar2FAParaCliente INT NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParametroSistema') AND name = 'Habilitar2FAParaAdministrador')
BEGIN
    ALTER TABLE ParametroSistema ADD Habilitar2FAParaAdministrador INT NULL;
END
