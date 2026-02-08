-- Tabela para registro de comunicações de token 2FA enviadas (e-mail e SMS).
-- Permite auditoria e gerenciamento de volume de mensagens enviadas pelo portal.
-- Execute em ambiente SQL Server. Ajuste se usar outro SGBD.

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComunicacaoTokenEnviada')
BEGIN
    CREATE TABLE ComunicacaoTokenEnviada (
        Id INT IDENTITY(1,1) NOT NULL,
        ObjectGuid NVARCHAR(100) NULL,
        UsuarioCriacao INT NULL,
        DataHoraCriacao DATETIME NULL,
        DataHoraAlteracao DATETIME NULL,
        UsuarioAlteracao INT NULL,
        Usuario INT NULL,
        Login NVARCHAR(200) NULL,
        Canal NVARCHAR(20) NOT NULL,
        Destinatario NVARCHAR(500) NULL,
        TextoEnviado NVARCHAR(MAX) NULL,
        DataHoraEnvio DATETIME NOT NULL,
        TwoFactorId UNIQUEIDENTIFIER NULL,
        EmailId INT NULL,
        CONSTRAINT PK_ComunicacaoTokenEnviada PRIMARY KEY (Id),
        CONSTRAINT FK_ComunicacaoTokenEnviada_Usuario FOREIGN KEY (Usuario) REFERENCES Usuario(Id)
    );

    CREATE INDEX IX_ComunicacaoTokenEnviada_DataHoraEnvio ON ComunicacaoTokenEnviada (DataHoraEnvio);
    CREATE INDEX IX_ComunicacaoTokenEnviada_Canal ON ComunicacaoTokenEnviada (Canal);
    CREATE INDEX IX_ComunicacaoTokenEnviada_Usuario ON ComunicacaoTokenEnviada (Usuario);
    CREATE INDEX IX_ComunicacaoTokenEnviada_Login ON ComunicacaoTokenEnviada (Login);
END
GO
