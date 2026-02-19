using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Attributes;
using SW_PortalProprietario.Application.Interfaces.Saga;
using SW_PortalProprietario.Application.Services.Core.Saga;

namespace SW_PortalProprietario.API.Examples
{
    /// <summary>
    /// Exemplo de como usar Saga Pattern em controllers
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SagaExampleController : ControllerBase
    {
        private readonly ISagaOrchestrator _sagaOrchestrator;
        private readonly ILogger<SagaExampleController> _logger;

        public SagaExampleController(
            ISagaOrchestrator sagaOrchestrator,
            ILogger<SagaExampleController> logger)
        {
            _sagaOrchestrator = sagaOrchestrator;
            _logger = logger;
        }

        /// <summary>
        /// Exemplo 1: Uso bÃ¡sico com ExecuteAsync
        /// </summary>
        [HttpPost("exemplo-basico")]
        [UseSaga("ExemploBasico")]
        public async Task<IActionResult> ExemploBasico([FromBody] ExemploRequest request)
        {
            var result = await _sagaOrchestrator.ExecuteAsync(
                "ExemploBasico",
                request,
                async (input, ct) =>
                {
                    // Step 1: Validar dados
                    await _sagaOrchestrator.ExecuteStepAsync(
                        "ValidarDados",
                        1,
                        input,
                        async (inp, ct) =>
                        {
                            _logger.LogInformation("Validando dados...");
                            await Task.Delay(100, ct);
                            // ValidaÃ§Ã£o aqui
                        },
                        compensateFunc: async (inp, ct) =>
                        {
                            _logger.LogInformation("Compensando validaÃ§Ã£o...");
                            // Nada a fazer neste caso
                        },
                        ct);

                    // Step 2: Processar pagamento
                    var pagamentoId = await _sagaOrchestrator.ExecuteStepAsync(
                        "ProcessarPagamento",
                        2,
                        input,
                        async (inp, ct) =>
                        {
                            _logger.LogInformation("Processando pagamento...");
                            await Task.Delay(200, ct);
                            return Guid.NewGuid().ToString(); // ID do pagamento
                        },
                        compensateFunc: async (inp, pagId, ct) =>
                        {
                            _logger.LogWarning("Estornando pagamento {PagamentoId}...", pagId);
                            await Task.Delay(100, ct);
                            // Estornar pagamento aqui
                        },
                        ct);

                    // Step 3: Criar reserva
                    var reservaId = await _sagaOrchestrator.ExecuteStepAsync(
                        "CriarReserva",
                        3,
                        new { input, pagamentoId },
                        async (inp, ct) =>
                        {
                            _logger.LogInformation("Criando reserva...");
                            await Task.Delay(150, ct);
                            return Guid.NewGuid().ToString(); // ID da reserva
                        },
                        compensateFunc: async (inp, resId, ct) =>
                        {
                            _logger.LogWarning("Cancelando reserva {ReservaId}...", resId);
                            await Task.Delay(100, ct);
                            // Cancelar reserva aqui
                        },
                        ct);

                    // Step 4: Enviar confirmaÃ§Ã£o
                    await _sagaOrchestrator.ExecuteStepAsync(
                        "EnviarConfirmacao",
                        4,
                        new { reservaId, email = input.Email },
                        async (inp, ct) =>
                        {
                            _logger.LogInformation("Enviando email de confirmaÃ§Ã£o...");
                            await Task.Delay(100, ct);
                            // Enviar email aqui
                        },
                        compensateFunc: null, // Email nÃ£o precisa ser compensado
                        ct);

                    return new ExemploResponse
                    {
                        Success = true,
                        ReservaId = reservaId,
                        PagamentoId = pagamentoId,
                        Message = "OperaÃ§Ã£o concluÃ­da com sucesso!"
                    };
                });

            return Ok(result);
        }

        /// <summary>
        /// Exemplo 2: Uso com SagaBuilder (fluent API)
        /// </summary>
        [HttpPost("exemplo-builder")]
        [UseSaga("ExemploBuilder")]
        public async Task<IActionResult> ExemploBuilder([FromBody] ExemploRequest request)
        {
            var result = await _sagaOrchestrator
                .CreateSaga("ExemploBuilder", request)
                .AddStep(
                    "ValidarDados",
                    async (input, ct) =>
                    {
                        _logger.LogInformation("Validando dados...");
                        await Task.Delay(100, ct);
                    })
                .AddStep<string>(
                    "ProcessarPagamento",
                    async (input, ct) =>
                    {
                        _logger.LogInformation("Processando pagamento...");
                        await Task.Delay(200, ct);
                        return Guid.NewGuid().ToString();
                    },
                    compensateFunc: async (input, pagId, ct) =>
                    {
                        _logger.LogWarning("Estornando pagamento {PagamentoId}...", pagId);
                        await Task.Delay(100, ct);
                    })
                .AddStep<string>(
                    "CriarReserva",
                    async (input, ct) =>
                    {
                        _logger.LogInformation("Criando reserva...");
                        await Task.Delay(150, ct);
                        return Guid.NewGuid().ToString();
                    },
                    compensateFunc: async (input, resId, ct) =>
                    {
                        _logger.LogWarning("Cancelando reserva {ReservaId}...", resId);
                        await Task.Delay(100, ct);
                    })
                .ExecuteAsync(
                    async input => new ExemploResponse
                    {
                        Success = true,
                        Message = "OperaÃ§Ã£o concluÃ­da com sucesso!"
                    });

            return Ok(result);
        }

        /// <summary>
        /// Exemplo 3: Simulando falha para testar compensaÃ§Ã£o
        /// </summary>
        [HttpPost("exemplo-falha")]
        [UseSaga("ExemploFalha")]
        public async Task<IActionResult> ExemploFalha([FromBody] ExemploRequest request)
        {
            try
            {
                var result = await _sagaOrchestrator.ExecuteAsync(
                    "ExemploFalha",
                    request,
                    async (input, ct) =>
                    {
                        // Step 1: Sucesso
                        await _sagaOrchestrator.ExecuteStepAsync(
                            "Step1_Sucesso",
                            1,
                            input,
                            async (inp, ct) =>
                            {
                                _logger.LogInformation("Step 1 executado");
                                await Task.Delay(100, ct);
                            },
                            compensateFunc: async (inp, ct) =>
                            {
                                _logger.LogWarning("Compensando Step 1");
                            },
                            ct);

                        // Step 2: Sucesso
                        await _sagaOrchestrator.ExecuteStepAsync(
                            "Step2_Sucesso",
                            2,
                            input,
                            async (inp, ct) =>
                            {
                                _logger.LogInformation("Step 2 executado");
                                await Task.Delay(100, ct);
                            },
                            compensateFunc: async (inp, ct) =>
                            {
                                _logger.LogWarning("Compensando Step 2");
                            },
                            ct);

                        // Step 3: FALHA PROPOSITAL
                        await _sagaOrchestrator.ExecuteStepAsync(
                            "Step3_Falha",
                            3,
                            input,
                            async (inp, ct) =>
                            {
                                _logger.LogInformation("Step 3 executado - vai falhar!");
                                await Task.Delay(50, ct);
                                throw new Exception("Falha simulada no Step 3!");
                            },
                            compensateFunc: async (inp, ct) =>
                            {
                                _logger.LogWarning("Compensando Step 3 (nÃ£o serÃ¡ executado)");
                            },
                            ct);

                        return new ExemploResponse { Success = true };
                    });

                return Ok(result);
            }
            catch (SagaException ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "OperaÃ§Ã£o falhou e foi compensada",
                    SagaId = ex.SagaId,
                    Error = ex.Message
                });
            }
        }
    }

    public class ExemploRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Valor { get; set; }
    }

    public class ExemploResponse
    {
        public bool Success { get; set; }
        public string? ReservaId { get; set; }
        public string? PagamentoId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
