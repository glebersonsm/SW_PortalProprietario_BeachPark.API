-- Script para adicionar as colunas OcupacaoMaxRetDispTS e OcupacaoMaxRetDispMP na tabela ConfigReservaVhf
-- Execute este script no banco de dados antes de executar a aplicação
-- Percentual máximo de ocupação para retorno de disponibilidade (0-100). Se atingido, não retorna disponibilidade.

-- Adicionar coluna OcupacaoMaxRetDispTS (Timesharing)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('portalohana.ConfigReservaVhf') AND name = 'OcupacaoMaxRetDispTS')
BEGIN
    ALTER TABLE portalohana.ConfigReservaVhf ADD OcupacaoMaxRetDispTS DECIMAL(5,2) NULL;
    PRINT 'Coluna OcupacaoMaxRetDispTS adicionada na tabela portalohana.ConfigReservaVhf';
END
ELSE
BEGIN
    PRINT 'Coluna OcupacaoMaxRetDispTS já existe na tabela portalohana.ConfigReservaVhf';
END
GO

-- Adicionar coluna OcupacaoMaxRetDispMP (Multipropriedade)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('portalohana.ConfigReservaVhf') AND name = 'OcupacaoMaxRetDispMP')
BEGIN
    ALTER TABLE portalohana.ConfigReservaVhf ADD OcupacaoMaxRetDispMP DECIMAL(5,2) NULL;
    PRINT 'Coluna OcupacaoMaxRetDispMP adicionada na tabela portalohana.ConfigReservaVhf';
END
ELSE
BEGIN
    PRINT 'Coluna OcupacaoMaxRetDispMP já existe na tabela portalohana.ConfigReservaVhf';
END
GO
