-- Rastreio de abertura de e-mail (tracking pixel).
-- Quando o destinatário carrega as imagens do e-mail, a primeira abertura é registrada aqui.

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Email') AND name = 'DataHoraPrimeiraAbertura')
BEGIN
    ALTER TABLE Email ADD DataHoraPrimeiraAbertura DATETIME2 NULL;
END
