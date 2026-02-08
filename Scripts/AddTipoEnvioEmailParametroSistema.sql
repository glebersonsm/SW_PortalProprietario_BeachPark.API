-- Migração: adicionar coluna TipoEnvioEmail em ParametroSistema
-- 0 = Cliente de email direto (MailKit), 1 = Cliente de email APP (System.Net.Mail).

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParametroSistema') AND name = 'TipoEnvioEmail')
BEGIN
    ALTER TABLE ParametroSistema ADD TipoEnvioEmail INT NULL;
END
