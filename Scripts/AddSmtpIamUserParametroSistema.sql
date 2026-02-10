-- Migração: adicionar coluna SmtpIamUser em ParametroSistema (nome do usuário IAM para envio AWS SES SMTP).

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParametroSistema') AND name = 'SmtpIamUser')
BEGIN
    ALTER TABLE ParametroSistema ADD SmtpIamUser NVARCHAR(500) NULL;
END
