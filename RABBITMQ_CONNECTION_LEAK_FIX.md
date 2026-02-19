# ?? Problema: 100+ Conexões RabbitMQ ao Subir a API

## ? Problema Identificado

Ao subir a API, foram criadas mais de **100 conexões simultâneas** no RabbitMQ, causando:
- Esgotamento de recursos do broker
- Lentidão na aplicação
- Possíveis timeouts e falhas

## ?? Causa Raiz

### 1. **Producers criando conexões a cada mensagem**

**Antes (INCORRETO):**
```csharp
public class RabbitMQAuditLogProducer : IAuditLogQueueProducer
{
    public async Task EnqueueAuditLogAsync(AuditLogMessageEvent message)
    {
        // ? Cria uma NOVA conexão A CADA mensagem publicada!
        var factory = new ConnectionFactory { ... };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        
        // ... publica mensagem ...
        
        channel.Dispose();
        connection.Dispose(); // ? Fecha, mas não libera imediatamente
    }
}
```

**Problema:** 
- Cada requisição HTTP que gera log/auditoria cria uma nova conexão
- 100 requisições simultâneas = 100 conexões
- Conexões TCP não são liberadas instantaneamente (TIME_WAIT, handshake, etc.)

### 2. **Todos os 3 Producers com o mesmo problema**

- `RabbitMQAuditLogProducer` - Logs de auditoria
- `RabbitMQRegisterLogMessageToQueueProducer` - Logs de acesso
- `RabbitMQEmailToQueueProducer` - Emails

Cada um criava conexões independentes a cada chamada.

### 3. **Registro como Singleton mas comportamento era Transient**

```csharp
// IoC: Singleton (1 instância do producer)
services.TryAddSingleton<IAuditLogQueueProducer, RabbitMQAuditLogProducer>();

// Mas dentro: Criava novas conexões (comportamento Transient)
public async Task EnqueueAuditLogAsync(...)
{
    var factory = new ConnectionFactory { ... }; // ? Nova cada vez
    using var connection = await factory.CreateConnectionAsync(); // ? Nova cada vez
}
```

## ? Solução Implementada

### **RabbitMQ Connection Manager** - Pool de Conexões Compartilhadas

Criado um gerenciador central que mantém:
- **1 conexão compartilhada para Producers**
- **1 conexão por Consumer** (long-running)
- Reutilização de conexões ao invés de criar novas

#### Arquitetura da Solução

```
????????????????????????????????????????????????????????????
?         RabbitMQConnectionManager (Singleton)            ?
????????????????????????????????????????????????????????????
?  ??????????????????????????????????????????????????????  ?
?  ? Producer Connection (shared)                        ?  ?
?  ? ?? Channel 1 (created per message, closed after)   ?  ?
?  ? ?? Channel 2 (created per message, closed after)   ?  ?
?  ? ?? Channel N (created per message, closed after)   ?  ?
?  ??????????????????????????????????????????????????????  ?
?                                                            ?
?  ??????????????????????????????????????????????????????  ?
?  ? Consumer Connections (1 per consumer)               ?  ?
?  ? ?? Consumer_AuditLog                                ?  ?
?  ? ?? Consumer_Log                                     ?  ?
?  ? ?? Consumer_Email                                   ?  ?
?  ??????????????????????????????????????????????????????  ?
????????????????????????????????????????????????????????????
```

#### Implementação

**`RabbitMQConnectionManager.cs`:**
```csharp
public class RabbitMQConnectionManager : IRabbitMQConnectionManager
{
    private IConnection? _producerConnection; // ? Conexão compartilhada
    private readonly SemaphoreSlim _producerLock = new(1, 1);

    public async Task<IConnection> GetProducerConnectionAsync()
    {
        // ? Retorna conexão existente se ainda estiver aberta
        if (_producerConnection != null && _producerConnection.IsOpen)
            return _producerConnection;

        await _producerLock.WaitAsync();
        try
        {
            if (_producerConnection != null && _producerConnection.IsOpen)
                return _producerConnection;

            // ? Cria apenas 1 vez quando necessário
            var factory = CreateConnectionFactory("ProducerConnection");
            _producerConnection = await factory.CreateConnectionAsync();
            
            return _producerConnection;
        }
        finally
        {
            _producerLock.Release();
        }
    }

    public async Task<IChannel> CreateChannelAsync(IConnection connection)
    {
        // ? Channels são leves e podem ser criados/descartados
        return await connection.CreateChannelAsync();
    }
}
```

**Producer atualizado:**
```csharp
public class RabbitMQAuditLogProducer : IAuditLogQueueProducer
{
    private readonly IRabbitMQConnectionManager _connectionManager;

    public async Task EnqueueAuditLogAsync(AuditLogMessageEvent message)
    {
        IChannel? channel = null;
        try
        {
            // ? Reutiliza conexão compartilhada
            var connection = await _connectionManager.GetProducerConnectionAsync();
            
            // ? Cria channel (leve) apenas para esta mensagem
            channel = await _connectionManager.CreateChannelAsync(connection);

            // ... publica mensagem ...
        }
        finally
        {
            // ? Fecha APENAS o channel, não a conexão
            if (channel != null)
            {
                await channel.CloseAsync();
                channel.Dispose();
            }
        }
    }
}
```

### Benefícios da Solução

| Antes | Depois |
|-------|--------|
| ? 1 conexão por mensagem | ? 1 conexão compartilhada |
| ? 100 requisições = 100 conexões | ? 100 requisições = 1 conexão + 100 channels |
| ? Conexões TCP não reutilizadas | ? Conexão TCP mantida aberta |
| ? Overhead de handshake a cada mensagem | ? Handshake apenas 1 vez |
| ? Esgotamento de recursos do broker | ? Uso eficiente de recursos |
| ? Possíveis timeouts | ? Performance estável |

## ?? Impacto Esperado

### Antes
```
API Startup:
?? Producer Auditoria: Cria 1 conexão por mensagem
?? Producer Log: Cria 1 conexão por mensagem  
?? Producer Email: Cria 1 conexão por mensagem
?? 100 requisições simultâneas = ~300 conexões!
```

### Depois
```
API Startup:
?? RabbitMQConnectionManager:
?   ?? Producer Connection (shared) ? 1 conexão
?   ?? Consumer_AuditLog ? 1 conexão
?   ?? Consumer_Log ? 1 conexão
?   ?? Consumer_Email ? 1 conexão
?? Total: 4 conexões (máximo)
```

## ?? Mudanças Realizadas

### Arquivos Criados
- ? `SW_PortalProprietario.Infra.Data\RabbitMQ\RabbitMQConnectionManager.cs`

### Arquivos Modificados
- ? `RabbitMQAuditLogProducer.cs` - Usa connection manager
- ? `RabbitMQRegisterLogMessageToQueueProducer.cs` - Usa connection manager
- ? `RabbitMQEmailToQueueProducer.cs` - Usa connection manager
- ? `RabbitMQConfigurationExtension.cs` - Registra connection manager

### Registro no IoC
```csharp
// Connection Manager - Singleton para gerenciar pool de conexões
services.TryAddSingleton<IRabbitMQConnectionManager, RabbitMQConnectionManager>();

// Producers - Singleton com injeção do ConnectionManager
services.TryAddSingleton<IAuditLogQueueProducer, RabbitMQAuditLogProducer>();
services.TryAddSingleton<ILogMessageToQueueProducer, RabbitMQRegisterLogMessageToQueueProducer>();
services.TryAddSingleton<ISenderEmailToQueueProducer, RabbitMQEmailToQueueProducer>();
```

## ?? Como Testar

1. **Reiniciar a API:**
   ```bash
   dotnet run
   ```

2. **Verificar conexões no RabbitMQ Management:**
   - Acesse: `http://localhost:15672`
   - Navegue para: **Connections**
   - **Antes:** ~100+ conexões
   - **Depois:** ~4 conexões (1 producer + 3 consumers)

3. **Testar carga:**
   ```bash
   # Simular 100 requisições simultâneas
   for i in {1..100}; do
       curl -X POST https://localhost:46395/api/auth/login &
   done
   ```
   - Verificar que o número de conexões permanece baixo (~4)

4. **Logs da aplicação:**
   ```
   [INF] Conexão RabbitMQ Producer criada e compartilhada
   [INF] Conexão RabbitMQ Consumer 'AuditLog' criada e compartilhada
   [INF] Conexão RabbitMQ Consumer 'Log' criada e compartilhada
   [INF] Conexão RabbitMQ Consumer 'Email' criada e compartilhada
   ```

## ?? Considerações Importantes

### Connection Lifetime
- A conexão Producer é mantida aberta durante toda a vida da aplicação
- É automaticamente recuperada se cair (AutomaticRecoveryEnabled)
- Fechada apenas no Dispose da aplicação

### Thread Safety
- `SemaphoreSlim` garante que apenas 1 thread cria a conexão por vez
- Double-check locking para performance

### Heartbeat
```csharp
RequestedHeartbeat = TimeSpan.FromSeconds(60)
```
Mantém a conexão viva mesmo sem tráfego

### Channel Pooling
- Channels são leves e podem ser criados/descartados
- Não há pool de channels (overhead mínimo)
- RabbitMQ Client já otimiza internamente

## ?? Referências

- [RabbitMQ Best Practices - Connections](https://www.rabbitmq.com/connections.html)
- [RabbitMQ .NET Client API Guide](https://www.rabbitmq.com/dotnet-api-guide.html)
- [Connection Pooling in RabbitMQ](https://www.rabbitmq.com/api-guide.html#connection-pooling)

## ? Checklist de Validação

Após deploy, verificar:

- [ ] Número de conexões no RabbitMQ Management (~4)
- [ ] Logs não mostram erros de conexão
- [ ] Performance de envio de mensagens mantida
- [ ] Mensagens sendo consumidas corretamente
- [ ] Sem memory leaks (monitorar por algumas horas)
- [ ] AutomaticRecovery funcionando em caso de falha

## ?? Próximos Passos (Opcional)

### Melhorias Futuras
1. **Channel Pooling:** Se necessário, implementar pool de channels reutilizáveis
2. **Métricas:** Adicionar contadores de mensagens publicadas/consumidas
3. **Circuit Breaker:** Implementar pattern para falhas do broker
4. **Health Checks:** Adicionar endpoint para verificar status da conexão

### Monitoramento Recomendado
```csharp
// TODO: Adicionar métricas
public class RabbitMQMetrics
{
    public int ActiveConnections { get; set; }
    public long MessagesPublished { get; set; }
    public long MessagesConsumed { get; set; }
    public TimeSpan AveragePublishTime { get; set; }
}
```

---

**Resumo:** O problema de 100+ conexões foi resolvido implementando um **Connection Manager** que mantém uma **conexão compartilhada** para todos os producers, reduzindo de ~300 conexões para apenas ~4.
