-- Script para adicionar as colunas Cor e CorTexto nas tabelas GrupoDocumento e Documento
-- Execute este script no banco de dados antes de executar a aplicação

-- Adicionar coluna Cor na tabela GrupoDocumento
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('GrupoDocumento') AND name = 'Cor')
BEGIN
    ALTER TABLE GrupoDocumento ADD Cor VARCHAR(50) NULL;
    PRINT 'Coluna Cor adicionada na tabela GrupoDocumento';
END
ELSE
BEGIN
    PRINT 'Coluna Cor já existe na tabela GrupoDocumento';
END
GO

-- Adicionar coluna CorTexto na tabela GrupoDocumento
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('GrupoDocumento') AND name = 'CorTexto')
BEGIN
    ALTER TABLE GrupoDocumento ADD CorTexto VARCHAR(50) NULL;
    PRINT 'Coluna CorTexto adicionada na tabela GrupoDocumento';
END
ELSE
BEGIN
    PRINT 'Coluna CorTexto já existe na tabela GrupoDocumento';
END
GO

-- Adicionar coluna Cor na tabela Documento
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Documento') AND name = 'Cor')
BEGIN
    ALTER TABLE Documento ADD Cor VARCHAR(50) NULL;
    PRINT 'Coluna Cor adicionada na tabela Documento';
END
ELSE
BEGIN
    PRINT 'Coluna Cor já existe na tabela Documento';
END
GO

-- Adicionar coluna CorTexto na tabela Documento
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Documento') AND name = 'CorTexto')
BEGIN
    ALTER TABLE Documento ADD CorTexto VARCHAR(50) NULL;
    PRINT 'Coluna CorTexto adicionada na tabela Documento';
END
ELSE
BEGIN
    PRINT 'Coluna CorTexto já existe na tabela Documento';
END
GO
