# Exemplo de Uso - Configuração RabbitMQ com Prioridade .env

## Problema Anterior

O código buscava configurações diretamente do `IConfiguration`, o que ignorava valores no `.env`:

```csharp
? INCORRETO:
var host = _configuration.GetValue<string>("RabbitMqConnectionHost");
var port = _configuration.GetValue<int>("RabbitMqConnectionPort");
var programId = _configuration.GetValue<string>("ProgramId", "PORTALPROP_");
```

**Problema:** Se você definir `RABBITMQ_HOST=localhost` no `.env`, o código acima **ignoraria** e usaria o valor do `appsettings.json`.

## Solução Implementada

### Forma Atual (já implementada no código)

O código usa o padrão existente que **já prioriza** o `.env`:

```csharp
? CORRETO (forma atual):
var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") 
    ?? _configuration.GetValue<string>("RabbitMqConnectionHost");
    
var port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int envPort) 
    ? envPort 
    : _configuration.GetValue<int>("RabbitMqConnectionPort");
    
var programId = _configuration.GetValue<string>("ProgramId", "PORTALPROP_");
```

**Como funciona:**
1. `OverrideConfigurationWithEnvironmentVariables` já sobrescreve `ProgramId` com `PROGRAM_ID` do `.env`
2. Para RabbitMQ, o código verifica explicitamente `Environment.GetEnvironmentVariable` primeiro
3. Se não encontrar, usa o `IConfiguration` (que já foi sobrescrito pelo `.env` se existir)

### Forma Alternativa (usando helper novo)

Você também pode usar o helper criado para clareza:

```csharp
? ALTERNATIVA (com helper):
using SW_PortalCliente_BeachPark.API.Helpers;

var host = EnvironmentConfigurationHelper.GetConfigValue(
    _configuration, 
    "RabbitMqConnectionHost",
    "RABBITMQ_HOST"
);

var port = EnvironmentConfigurationHelper.GetConfigValue<int>(
    _configuration,
    "RabbitMqConnectionPort",
    "RABBITMQ_PORT",
    5672
);

var programId = EnvironmentConfigurationHelper.GetConfigValue(
    _configuration,
    "ProgramId",
    "PROGRAM_ID",
    "PORTALPROP_"
);
```

## Ordem de Busca Garantida

Para qualquer configuração, a ordem é **sempre**:

```
1. Variável de ambiente (.env)
   ? (se não encontrar)
2. appsettings.{Environment}.json
   ? (se não encontrar)
3. appsettings.json
   ? (se não encontrar)
4. Valor padrão no código
```

## Exemplo Completo - RabbitMQQueueService

### Configuração nos arquivos

**`.env` (desenvolvimento):**
```env
PROGRAM_ID=PortalClienteBP_
RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USER=swsolucoes
RABBITMQ_PASS=SW@dba#2024!
RABBITMQ_FILA_AUDITORIA=auditoria_bp
RABBITMQ_FILA_LOG=gravacaoLogs_mvc
RABBITMQ_FILA_EMAIL=emails_mvc
```

**`appsettings.json` (valores padrão/fallback):**
```json
{
  "ProgramId": "PORTALPROP_",
  "RabbitMqConnectionHost": "production-rabbit.example.com",
  "RabbitMqConnectionPort": 5672,
  "RabbitMqConnectionUser": "guest",
  "RabbitMqConnectionPass": "guest",
  "RabbitMqFilaDeAuditoriaNome": "auditoria_bp",
  "RabbitMqFilaDeLogNome": "gravacaoLogs_mvc",
  "RabbitMqFilaDeEmailNome": "emails_mvc"
}
```

### Código (forma atual já implementada)

```csharp
private async Task SyncQueuesFromRabbitMQ()
{
    // ? Busca com precedência .env > appsettings.json
    var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") 
        ?? _configuration.GetValue<string>("RabbitMqConnectionHost");
    
    var port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int envPort) 
        ? envPort 
        : _configuration.GetValue<int>("RabbitMqConnectionPort");
    
    var user = Environment.GetEnvironmentVariable("RABBITMQ_USER") 
        ?? _configuration.GetValue<string>("RabbitMqConnectionUser");
    
    var pass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") 
        ?? _configuration.GetValue<string>("RabbitMqConnectionPass");
    
    // ProgramId já foi sobrescrito pelo OverrideConfigurationWithEnvironmentVariables
    var programId = _configuration.GetValue<string>("ProgramId", "PORTALPROP_");
    
    _logger.LogInformation($"ProgramId configurado: '{programId}'");
    
    // Nomes das filas
    var queueAuditoriaNomeRaw = _configuration.GetValue<string>("RabbitMqFilaDeAuditoriaNome", "auditoria_bp");
    var queueLogNomeRaw = _configuration.GetValue<string>("RabbitMqFilaDeLogNome", "gravacaoLogs_mvc");
    var queueEmailNomeRaw = _configuration.GetValue<string>("RabbitMqFilaDeEmailNome", "emails_mvc");
    
    // Resultado com .env:
    // programId = "PortalClienteBP_"
    // host = "localhost"
    // port = 5672
    // user = "swsolucoes"
    // pass = "SW@dba#2024!"
    // queueAuditoriaNomeRaw = "auditoria_bp"
}
```

### Resultado dos Nomes das Filas

Com o `.env` acima, as filas serão criadas como:
- `PortalClienteBP_auditoria_bp` ?
- `PortalClienteBP_gravacaoLogs_mvc` ?
- `PortalClienteBP_emails_mvc` ?

**Não mais:** `PORTALCLIENTE_BPgravacaoLogs_mvc` ? (erro anterior)

## Verificação em Runtime

Para confirmar qual valor está sendo usado:

```csharp
_logger.LogInformation($"=== Configurações RabbitMQ ===");
_logger.LogInformation($"ProgramId: '{programId}'");
_logger.LogInformation($"Host: '{host}'");
_logger.LogInformation($"Port: {port}");
_logger.LogInformation($"User: '{user}'");
_logger.LogInformation($"Fila Auditoria: '{queueAuditoriaNomeRaw}'");
_logger.LogInformation($"Fila Log: '{queueLogNomeRaw}'");
_logger.LogInformation($"Fila Email: '{queueEmailNomeRaw}'");
_logger.LogInformation($"Nome completo fila log: '{programId}{queueLogNomeRaw}'");
```

## Casos de Uso

### Caso 1: Desenvolvimento Local
- **`.env`**: Configurações locais (localhost, portas locais, etc)
- **`appsettings.json`**: Valores padrão ignorados
- **Resultado**: Usa tudo do `.env`

### Caso 2: CI/CD
- **Variáveis de ambiente**: Injetadas pelo pipeline
- **`appsettings.json`**: Valores padrão ignorados
- **Resultado**: Usa variáveis do pipeline

### Caso 3: Produção (sem .env)
- **`.env`**: Não existe
- **`appsettings.Production.json`**: Configurações de produção
- **Resultado**: Usa appsettings

## Migração de Código Existente

Se você encontrar código assim:

```csharp
? var programId = _configuration.GetValue<string>("ProgramId", "PORTALPROP_");
```

**Não precisa alterar!** Está correto porque `OverrideConfigurationWithEnvironmentVariables` já sobrescreveu o valor no `IConfiguration`.

Se quiser deixar mais explícito:

```csharp
? var programId = EnvironmentConfigurationHelper.GetConfigValue(
    _configuration,
    "ProgramId",
    "PROGRAM_ID",
    "PORTALPROP_"
);
```

## Conclusão

O sistema **já funciona corretamente** priorizando `.env`. As melhorias adicionadas foram:

1. ? Helper `GetConfigValue` para uso opcional mais explícito
2. ? Logs detalhados mostrando valores usados
3. ? Documentação clara da precedência
4. ? Mapeamento de variáveis RabbitMQ no `OverrideConfigurationWithEnvironmentVariables`
5. ? Correção do `PROGRAM_ID` no `.env` (estava sem `_` no final)

**Não é necessário alterar código existente**, apenas seguir o padrão ao adicionar novas configurações.
