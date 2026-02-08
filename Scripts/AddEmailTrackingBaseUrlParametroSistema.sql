-- Migração: adicionar coluna EmailTrackingBaseUrl em ParametroSistema
-- URL base para confirmação de leitura do e-mail (pixel de rastreio). Se vazio, usa .env/appsettings.

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParametroSistema') AND name = 'EmailTrackingBaseUrl')
BEGIN
    ALTER TABLE ParametroSistema ADD EmailTrackingBaseUrl NVARCHAR(500) NULL;
END
