-- Migração: adicionar coluna UsuarioAdministrativo na tabela Usuario.

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Usuario') AND name = 'UsuarioAdministrativo')
BEGIN
    ALTER TABLE Usuario ADD UsuarioAdministrativo INT NULL;
    
    -- Definir valor padrão como 0 (Não) para registros existentes
    UPDATE Usuario SET UsuarioAdministrativo = 0 WHERE UsuarioAdministrativo IS NULL;
END
