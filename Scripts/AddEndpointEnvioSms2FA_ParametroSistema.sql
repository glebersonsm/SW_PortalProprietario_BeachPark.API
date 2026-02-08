-- Migração: adicionar coluna para endpoint de envio SMS 2FA em ParametroSistema
-- Permite configurar a URL do serviço de SMS no cadastro, sem valor interno no código.

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParametroSistema') AND name = 'EndpointEnvioSms2FA')
BEGIN
    ALTER TABLE ParametroSistema ADD EndpointEnvioSms2FA NVARCHAR(500) NULL;
END
