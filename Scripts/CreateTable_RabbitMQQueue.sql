-- Script para criar a tabela RabbitMQQueue
-- Execute este script no banco de dados do Portal

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RabbitMQQueue]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RabbitMQQueue](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ObjectGuid] [nvarchar](100) NULL,
        [UsuarioCriacao] [int] NULL,
        [DataHoraCriacao] [datetime] NOT NULL,
        [DataHoraAlteracao] [datetime] NULL,
        [UsuarioAlteracao] [int] NULL,
        [Nome] [nvarchar](200) NOT NULL,
        [Descricao] [nvarchar](500) NULL,
        [Ativo] [int] NOT NULL DEFAULT 1,
        [TipoFila] [nvarchar](50) NOT NULL,
        [ExchangeName] [nvarchar](200) NULL,
        [RoutingKey] [nvarchar](200) NULL,
        [PrefetchCount] [int] NULL,
        [ConsumerConcurrency] [int] NULL,
        [RetryAttempts] [int] NULL,
        [RetryDelaySeconds] [int] NULL,
        CONSTRAINT [PK_RabbitMQQueue] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    PRINT 'Tabela RabbitMQQueue criada com sucesso!'
END
ELSE
BEGIN
    PRINT 'Tabela RabbitMQQueue já existe.'
END
GO

-- Criar índices para melhor performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RabbitMQQueue_Nome' AND object_id = OBJECT_ID('RabbitMQQueue'))
BEGIN
    CREATE INDEX IX_RabbitMQQueue_Nome ON RabbitMQQueue(Nome)
    PRINT 'Índice IX_RabbitMQQueue_Nome criado com sucesso!'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RabbitMQQueue_TipoFila' AND object_id = OBJECT_ID('RabbitMQQueue'))
BEGIN
    CREATE INDEX IX_RabbitMQQueue_TipoFila ON RabbitMQQueue(TipoFila)
    PRINT 'Índice IX_RabbitMQQueue_TipoFila criado com sucesso!'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RabbitMQQueue_Ativo' AND object_id = OBJECT_ID('RabbitMQQueue'))
BEGIN
    CREATE INDEX IX_RabbitMQQueue_Ativo ON RabbitMQQueue(Ativo)
    PRINT 'Índice IX_RabbitMQQueue_Ativo criado com sucesso!'
END
GO
