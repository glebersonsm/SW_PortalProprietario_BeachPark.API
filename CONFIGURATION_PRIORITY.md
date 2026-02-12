# Prioridade de Configuração - .env vs appsettings.json

## Ordem de Precedência

A aplicação segue a seguinte ordem de prioridade ao buscar valores de configuração:

1. **`.env` (Variáveis de Ambiente)** - PRIORIDADE MÁXIMA
2. **`appsettings.{Environment}.json`** - Fallback
3. **`appsettings.json`** - Fallback final
4. **Valor padrão no código** - Se nenhum dos anteriores existir

## Como Funciona

### 1. Carregamento Inicial (Program.cs)
```csharp
// 1. Carrega .env
Env.Load(envPath);

// 2. Carrega JSONs
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{Environment}.json", optional: true);

// 3. Sobrescreve com variáveis de ambiente
EnvironmentConfigurationHelper.OverrideConfigurationWithEnvironmentVariables(builder.Configuration);
```

### 2. Como Usar no Código

**? FORMA CORRETA - Usar o helper (prioriza .env automaticamente):**
```csharp
using SW_PortalCliente_BeachPark.API.Helpers;

// String
var host = EnvironmentConfigurationHelper.GetConfigValue(
    _configuration, 
    "RabbitMqConnectionHost",  // chave no appsettings.json
    "RABBITMQ_HOST",            // variável no .env
    "localhost"                 // valor padrão
);

// Tipado (int, bool, etc)
var port = EnvironmentConfigurationHelper.GetConfigValue<int>(
    _configuration,
    "RabbitMqConnectionPort",
    "RABBITMQ_PORT",
    5672
);
```

**? FORMA INCORRETA - Buscar direto (ignora .env):**
```csharp
// Isso busca APENAS no appsettings.json!
var host = _configuration.GetValue<string>("RabbitMqConnectionHost");

// Mesmo isso não garante precedência:
var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") 
    ?? _configuration.GetValue<string>("RabbitMqConnectionHost");
```

## Mapeamento de Variáveis

### RabbitMQ
| Chave no appsettings.json | Variável no .env | Padrão |
|---------------------------|------------------|--------|
| `RabbitMqConnectionHost` | `RABBITMQ_HOST` | - |
| `RabbitMqConnectionPort` | `RABBITMQ_PORT` | `5672` |
| `RabbitMqConnectionUser` | `RABBITMQ_USER` | `guest` |
| `RabbitMqConnectionPass` | `RABBITMQ_PASS` | - |
| `RabbitMqFilaDeAuditoriaNome` | `RABBITMQ_FILA_AUDITORIA` | `auditoria_bp` |
| `RabbitMqFilaDeLogNome` | `RABBITMQ_FILA_LOG` | `gravacaoLogs_mvc` |
| `RabbitMqFilaDeEmailNome` | `RABBITMQ_FILA_EMAIL` | `emails_mvc` |
| `ProgramId` | `PROGRAM_ID` | `PORTALPROP_` |

### Conexões
| Chave | Variável | Padrão |
|-------|----------|--------|
| `ConnectionStrings:DefaultConnection` | `DEFAULT_CONNECTION` | - |
| `ConnectionStrings:CmConnection` | `CM_CONNECTION` | - |
| `ConnectionStrings:EsolAccessCenterConnection` | `ESOL_ACCESS_CENTER_CONNECTION` | - |
| `ConnectionStrings:EsolPortalConnection` | `ESOL_PORTAL_CONNECTION` | - |
| `ConnectionStrings:RedisConnection` | `REDIS_CONNECTION` | - |

### Redis
| Chave | Variável | Padrão |
|-------|----------|--------|
| `Redis:Password` | `REDIS_PASSWORD` | - |
| `Redis:Hosts:0:Host` | `REDIS_HOST` | `localhost` |
| `Redis:Hosts:0:Port` | `REDIS_PORT` | `6379` |
| `Redis:Database` | `REDIS_DATABASE` | `0` |

### JWT
| Chave | Variável | Padrão |
|-------|----------|--------|
| `Jwt:Key` | `JWT_KEY` | - |
| `Jwt:Issuer` | `JWT_ISSUER` | - |
| `Jwt:Audience` | `JWT_AUDIENCE` | - |

## Exemplos Práticos

### Exemplo 1: Ambiente de Desenvolvimento
```env
# .env (desenvolvimento local)
RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
PROGRAM_ID=PortalClienteBP_Dev_
```

```json
// appsettings.json (ignorado se existir no .env)
{
  "RabbitMqConnectionHost": "production-rabbit.example.com",
  "RabbitMqConnectionPort": 5672,
  "ProgramId": "PortalClienteBP_"
}
```

**Resultado:** Usa `localhost` e `PortalClienteBP_Dev_` do `.env`

### Exemplo 2: Ambiente de Produção
```env
# .env (produção - vazio ou sem RabbitMQ)
DEFAULT_CONNECTION=Host=prod-db;Database=portal;...
```

```json
// appsettings.Production.json
{
  "RabbitMqConnectionHost": "production-rabbit.example.com",
  "RabbitMqConnectionPort": 5672,
  "ProgramId": "PortalClienteBP_"
}
```

**Resultado:** Usa valores do `appsettings.Production.json` pois não existem no `.env`

## Benefícios

1. **Desenvolvimento Local:** Cada desenvolvedor pode ter seu próprio `.env` sem commitar no Git
2. **CI/CD:** Injetar variáveis de ambiente no pipeline sem alterar código
3. **Segurança:** Senhas e secrets no `.env` (que está no `.gitignore`)
4. **Flexibilidade:** Configurações diferentes por ambiente sem múltiplos arquivos JSON
5. **Prioridade Clara:** Sempre sabemos onde alterar (`.env` primeiro, JSON depois)

## Checklist para Novos Valores de Configuração

Ao adicionar uma nova configuração:

- [ ] Adicionar variável no `.env.example` com comentário
- [ ] Adicionar chave no `appsettings.json` com valor padrão
- [ ] Adicionar mapeamento em `EnvironmentConfigurationHelper.OverrideConfigurationWithEnvironmentVariables`
- [ ] Documentar neste arquivo a correspondência
- [ ] Usar o helper `GetConfigValue` no código ao invés de `IConfiguration` diretamente

## Troubleshooting

**Problema:** Minha alteração no `.env` não está sendo lida

**Soluções:**
1. Reinicie a aplicação (o `.env` é carregado no startup)
2. Verifique se o arquivo `.env` está na raiz do projeto
3. Verifique se a variável está sendo sobrescrita em `OverrideConfigurationWithEnvironmentVariables`
4. Use logs para confirmar qual valor está sendo usado:
   ```csharp
   _logger.LogInformation($"RabbitMQ Host: {host}");
   ```

**Problema:** Não sei se está usando `.env` ou `appsettings.json`

**Solução:**
Use o método helper que já implementa a lógica de precedência:
```csharp
var value = EnvironmentConfigurationHelper.GetConfigValue(
    _configuration, 
    "ChaveNoJson", 
    "VARIAVEL_NO_ENV"
);
```

## Ver Também

- `.env.example` - Template de variáveis disponíveis
- `src/Helpers/EnvironmentConfigurationHelper.cs` - Implementação da lógica
- `Program.cs` - Carregamento inicial das configurações
