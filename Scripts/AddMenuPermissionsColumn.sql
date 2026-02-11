-- Migração: adicionar coluna MenuPermissions na tabela Usuario para armazenar permissões de menu como JSON.

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Usuario') AND name = 'MenuPermissions')
BEGIN
    ALTER TABLE Usuario ADD MenuPermissions NVARCHAR(MAX) NULL;
END
