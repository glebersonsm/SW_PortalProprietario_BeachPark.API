-- Migração: adicionar colunas de configuração SMTP em ParametroSistema
-- Permite configurar envio de e-mail pelo sistema (parâmetros) em vez de .env/appsettings.

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParametroSistema') AND name = 'SmtpHost')
BEGIN
    ALTER TABLE ParametroSistema ADD SmtpHost NVARCHAR(500) NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParametroSistema') AND name = 'SmtpPort')
BEGIN
    ALTER TABLE ParametroSistema ADD SmtpPort INT NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParametroSistema') AND name = 'SmtpUseSsl')
BEGIN
    ALTER TABLE ParametroSistema ADD SmtpUseSsl INT NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParametroSistema') AND name = 'SmtpUser')
BEGIN
    ALTER TABLE ParametroSistema ADD SmtpUser NVARCHAR(500) NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParametroSistema') AND name = 'SmtpPass')
BEGIN
    ALTER TABLE ParametroSistema ADD SmtpPass NVARCHAR(500) NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParametroSistema') AND name = 'SmtpFromName')
BEGIN
    ALTER TABLE ParametroSistema ADD SmtpFromName NVARCHAR(500) NULL;
END
