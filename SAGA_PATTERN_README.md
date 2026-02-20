# Sistema de TransaÃ§Ãµes DistribuÃ­das (Saga Pattern)

## ðŸ“‹ VisÃ£o Geral

Este sistema implementa o **Saga Pattern** para gerenciar transaÃ§Ãµes distribuÃ­das na API, garantindo que:
- âœ… Todas as operaÃ§Ãµes sejam rastreadas
- âœ… Em caso de falha, todas as operaÃ§Ãµes anteriores sejam compensadas (desfeitas)
- âœ… Todos os passos executados sejam registrados no banco de dados
- âœ… Seja possÃ­vel auditar e investigar problemas

## ðŸ—ï¸ Arquitetura

### Componentes Principais

1. **SagaExecution** - Entidade que representa uma execuÃ§Ã£o completa de Saga
2. **SagaStep** - Entidade que representa um passo individual dentro de uma Saga
3. **ISagaOrchestrator** - Interface para orquestrar Sagas
4. **SagaOrchestrator** - ImplementaÃ§Ã£o do orquestrador
5. **ISagaRepository** - Interface para persistir logs de Saga
6. **SagaRepository** - ImplementaÃ§Ã£o do repositÃ³rio
7. **UseSagaAttribute** - Atributo para marcar endpoints que usam Saga
8. **SagaMiddleware** - Middleware para interceptar requisiÃ§Ãµes

### Fluxo de ExecuÃ§Ã£o

```
1. RequisiÃ§Ã£o chega ao endpoint marcado com [UseSaga]
2. Middleware detecta o atributo
3. SagaOrchestrator cria uma nova SagaExecution
4. Cada step Ã© executado e registrado como SagaStep
5. Se algum step falhar:
   - Todos os steps anteriores sÃ£o compensados (em ordem reversa)
   - Cada compensaÃ§Ã£o Ã© registrada
6. Status final Ã© atualizado (Completed ou Compensated)
```

## ðŸš€ Como Usar

### 1. ConfiguraÃ§Ã£o Inicial

No `Program.cs` ou `Startup.cs`:

```csharp
// Registrar serviÃ§os de Saga
builder.Services.AddSagaPattern();

// Adicionar middleware (apÃ³s UseRouting, antes de UseEndpoints)
app.UseSagaMiddleware();
```

### 2. Uso BÃ¡sico em Controllers

#### OpÃ§Ã£o A: Uso Direto com ExecuteAsync

```csharp
[HttpPost("criar-reserva")]
[UseSaga("CriarReserva")]
public async Task<IActionResult> CriarReserva([FromBody] ReservaRequest request)
{
    var result = await _sagaOrchestrator.ExecuteAsync(
        "CriarReserva",
        request,
        async (input, ct) =>
        {
            // Step 1: Validar disponibilidade
            await _sagaOrchestrator.ExecuteStepAsync(
                "ValidarDisponibilidade",
                1,
                input,
                async (inp, ct) => 
                {
                    // LÃ³gica de validaÃ§Ã£o
                    await _servicoReserva.ValidarDisponibilidadeAsync(inp.QuartoId, inp.DataCheckIn);
                },
                compensateFunc: null, // ValidaÃ§Ã£o nÃ£o precisa compensaÃ§Ã£o
                ct);

            // Step 2: Processar pagamento
            var pagamentoId = await _sagaOrchestrator.ExecuteStepAsync(
                "ProcessarPagamento",
                2,
                input,
                async (inp, ct) => 
                {
                    // Processar pagamento
                    return await _servicoPagamento.ProcessarAsync(inp.CartaoId, inp.Valor);
                },
                compensateFunc: async (inp, pagId, ct) => 
                {
                    // COMPENSAÃ‡ÃƒO: Estornar pagamento
                    await _servicoPagamento.EstornarAsync(pagId);
                },
                ct);

            // Step 3: Criar reserva
            var reservaId = await _sagaOrchestrator.ExecuteStepAsync(
                "CriarReserva",
                3,
                new { input, pagamentoId },
                async (inp, ct) => 
                {
                    // Criar reserva
                    return await _servicoReserva.CriarAsync(inp.input, inp.pagamentoId);
                },
                compensateFunc: async (inp, resId, ct) => 
                {
                    // COMPENSAÃ‡ÃƒO: Cancelar reserva
                    await _servicoReserva.CancelarAsync(resId);
                },
                ct);

            // Step 4: Enviar confirmaÃ§Ã£o
            await _sagaOrchestrator.ExecuteStepAsync(
                "EnviarConfirmacao",
                4,
                new { reservaId, input.Email },
                async (inp, ct) => 
                {
                    // Enviar email
                    await _servicoEmail.EnviarConfirmacaoAsync(inp.Email, inp.reservaId);
                },
                compensateFunc: null, // Email nÃ£o precisa compensaÃ§Ã£o
                ct);

            return new ReservaResponse 
            { 
                ReservaId = reservaId,
                PagamentoId = pagamentoId 
            };
        });

    return Ok(result);
}
```

#### OpÃ§Ã£o B: Uso com SagaBuilder (Fluent API)

```csharp
[HttpPost("criar-reserva")]
[UseSaga("CriarReserva")]
public async Task<IActionResult> CriarReserva([FromBody] ReservaRequest request)
{
    var result = await _sagaOrchestrator
        .CreateSaga("CriarReserva", request)
        .AddStep(
            "ValidarDisponibilidade",
            async (input, ct) => await _servicoReserva.ValidarDisponibilidadeAsync(input.QuartoId, input.DataCheckIn))
        .AddStep<string>(
            "ProcessarPagamento",
            async (input, ct) => await _servicoPagamento.ProcessarAsync(input.CartaoId, input.Valor),
            compensateFunc: async (input, pagId, ct) => await _servicoPagamento.EstornarAsync(pagId))
        .AddStep<int>(
            "CriarReserva",
            async (input, ct) => await _servicoReserva.CriarAsync(input),
            compensateFunc: async (input, resId, ct) => await _servicoReserva.CancelarAsync(resId))
        .ExecuteAsync(
            async input => new ReservaResponse 
            { 
                Success = true,
                Message = "Reserva criada com sucesso!" 
            });

    return Ok(result);
}
```

### 3. Tratamento de Erros

```csharp
try
{
    var result = await _sagaOrchestrator.ExecuteAsync(...);
    return Ok(result);
}
catch (SagaException ex)
{
    // Saga falhou e foi compensada
    _logger.LogError(ex, "Saga {SagaId} falhou", ex.SagaId);
    
    return BadRequest(new
    {
        Success = false,
        Message = "OperaÃ§Ã£o falhou e foi revertida",
        SagaId = ex.SagaId,
        Error = ex.Message
    });
}
```

## ðŸ“Š Estrutura do Banco de Dados

### Tabela: SagaExecution

| Campo | Tipo | DescriÃ§Ã£o |
|-------|------|-----------|
| Id | int | ID Ãºnico |
| SagaId | string | GUID da Saga |
| OperationType | string | Tipo da operaÃ§Ã£o |
| Status | string | Running, Completed, Compensated, Failed |
| InputData | string | JSON com dados de entrada |
| OutputData | string | JSON com resultado |
| ErrorMessage | string | Mensagem de erro |
| DataHoraInicio | DateTime | Quando iniciou |
| DataHoraConclusao | DateTime | Quando terminou |
| DuracaoMs | long | DuraÃ§Ã£o em milissegundos |
| UsuarioId | int | UsuÃ¡rio que iniciou |
| Endpoint | string | Endpoint da API |
| ClientIp | string | IP do cliente |

### Tabela: SagaStep

| Campo | Tipo | DescriÃ§Ã£o |
|-------|------|-----------|
| Id | int | ID Ãºnico |
| SagaExecutionId | int | FK para SagaExecution |
| StepName | string | Nome do step |
| StepOrder | int | Ordem de execuÃ§Ã£o |
| Status | string | Pending, Executing, Executed, Compensating, Compensated, Failed |
| InputData | string | JSON com entrada do step |
| OutputData | string | JSON com saÃ­da do step |
| ErrorMessage | string | Mensagem de erro |
| StackTrace | string | Stack trace do erro |
| DataHoraInicio | DateTime | Quando iniciou |
| DataHoraConclusao | DateTime | Quando terminou |
| DuracaoMs | long | DuraÃ§Ã£o em milissegundos |
| DataHoraInicioCompensacao | DateTime | Quando iniciou compensaÃ§Ã£o |
| DataHoraConclusaoCompensacao | DateTime | Quando terminou compensaÃ§Ã£o |
| DuracaoCompensacaoMs | long | DuraÃ§Ã£o da compensaÃ§Ã£o |
| Tentativas | int | NÃºmero de tentativas |
| TentativasCompensacao | int | NÃºmero de tentativas de compensaÃ§Ã£o |
| PodeSerCompensado | bool | Se pode ser compensado |

## ðŸ” Monitoramento e Auditoria

### Consultar Sagas por Status

```csharp
var sagasFalhadas = await _sagaRepository.GetSagasByStatusAsync("Failed", limit: 50);
var sagasCompensadas = await _sagaRepository.GetSagasByStatusAsync("Compensated", limit: 50);
```

### Consultar Sagas por Tipo de OperaÃ§Ã£o

```csharp
var reservas = await _sagaRepository.GetSagasByOperationTypeAsync(
    "CriarReserva",
    dataInicio: DateTime.Today.AddDays(-7),
    dataFim: DateTime.Today);
```

### Obter Detalhes de uma Saga

```csharp
var saga = await _sagaRepository.GetSagaAsync(sagaId);
var steps = await _sagaRepository.GetStepsAsync(sagaId);

foreach (var step in steps)
{
    Console.WriteLine($"{step.StepOrder}. {step.StepName} - {step.Status}");
    if (step.Status == "Failed")
    {
        Console.WriteLine($"   Erro: {step.ErrorMessage}");
    }
}
```

## ðŸ“ Logs

O sistema gera logs detalhados com emojis para fÃ¡cil identificaÃ§Ã£o:

- ðŸš€ Saga iniciada
- âš™ï¸ Step executando
- âœ“ Step executado com sucesso
- âœ— Step falhou
- ðŸ”„ Iniciando compensaÃ§Ã£o
- â†©ï¸ Compensando step
- âœ… Saga completada / CompensaÃ§Ã£o concluÃ­da
- âŒ Erro

Exemplo de log:
```
ðŸš€ Iniciando Saga abc123 - Tipo: CriarReserva
âš™ï¸ Executando step ValidarDisponibilidade (Ordem: 1) - Saga abc123
âœ“ Step ValidarDisponibilidade executado com sucesso em 45ms
âš™ï¸ Executando step ProcessarPagamento (Ordem: 2) - Saga abc123
âœ— Falha no step ProcessarPagamento apÃ³s 120ms
ðŸ”„ Iniciando compensaÃ§Ã£o de 1 steps executados
â†©ï¸ Compensando step ValidarDisponibilidade (Ordem: 1)
âœ“ Step ValidarDisponibilidade compensado com sucesso em 20ms
âœ… CompensaÃ§Ã£o concluÃ­da - 1 steps processados
```

## âš ï¸ Boas PrÃ¡ticas

1. **Sempre implemente compensaÃ§Ã£o** para steps que modificam estado
2. **Mantenha steps idempotentes** quando possÃ­vel
3. **Use ordem lÃ³gica** nos steps (1, 2, 3...)
4. **NÃ£o compense operaÃ§Ãµes de leitura** (validaÃ§Ãµes, consultas)
5. **Trate erros de compensaÃ§Ã£o** - o sistema continua mesmo se uma compensaÃ§Ã£o falhar
6. **Use nomes descritivos** para steps e operaÃ§Ãµes
7. **Serialize apenas dados necessÃ¡rios** para evitar logs muito grandes

## ðŸŽ¯ Casos de Uso Ideais

- âœ… CriaÃ§Ã£o de reservas com pagamento
- âœ… Processos de checkout multi-etapas
- âœ… TransferÃªncias entre contas
- âœ… OperaÃ§Ãµes que envolvem mÃºltiplos sistemas
- âœ… Workflows complexos com rollback
- âœ… IntegraÃ§Ãµes com APIs externas

## ðŸš« Quando NÃƒO Usar

- âŒ OperaÃ§Ãµes simples de CRUD
- âŒ Consultas sem modificaÃ§Ã£o de estado
- âŒ OperaÃ§Ãµes que jÃ¡ tÃªm transaÃ§Ã£o de banco de dados
- âŒ Processos sÃ­ncronos muito rÃ¡pidos (< 100ms)

## ðŸ“š ReferÃªncias

- [Saga Pattern - Microsoft](https://docs.microsoft.com/en-us/azure/architecture/reference-architectures/saga/saga)
- [Microservices Patterns - Chris Richardson](https://microservices.io/patterns/data/saga.html)
