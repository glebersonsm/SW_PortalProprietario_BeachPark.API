using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Attributes;
using SW_PortalProprietario.Application.Interfaces.Saga;
using System.Reflection;

namespace SW_PortalProprietario.Infra.Ioc.Middleware
{
    /// <summary>
    /// Middleware para interceptar requisições e aplicar Saga Pattern automaticamente
    /// quando o endpoint estiver marcado com [UseSaga]
    /// </summary>
    public class SagaMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SagaMiddleware> _logger;

        public SagaMiddleware(RequestDelegate next, ILogger<SagaMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context, ISagaOrchestrator sagaOrchestrator)
        {
            // Verifica se o endpoint tem o atributo [UseSaga]
            var endpoint = context.GetEndpoint();
            var sagaAttribute = endpoint?.Metadata.GetMetadata<UseSagaAttribute>();

            if (sagaAttribute == null)
            {
                // Não usa Saga, continua normalmente
                await _next(context);
                return;
            }

            _logger.LogInformation(
                "🔷 Requisição com Saga detectada: {Method} {Path} - Operação: {Operation}",
                context.Request.Method,
                context.Request.Path,
                sagaAttribute.OperationName);

            // Adiciona informações da Saga no contexto
            context.Items["UseSaga"] = true;
            context.Items["SagaOperationName"] = sagaAttribute.OperationName;
            context.Items["SagaOrchestrator"] = sagaOrchestrator;

            try
            {
                // Executa a requisição
                await _next(context);

                if (sagaOrchestrator.CurrentSagaId != null)
                {
                    _logger.LogInformation(
                        "✅ Requisição com Saga concluída: {SagaId}",
                        sagaOrchestrator.CurrentSagaId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Erro em requisição com Saga: {Operation}",
                    sagaAttribute.OperationName);

                if (sagaAttribute.ThrowOnFailure)
                    throw;
            }
        }
    }

    /// <summary>
    /// Extensões para registrar o middleware
    /// </summary>
    public static class SagaMiddlewareExtensions
    {
        public static IApplicationBuilder UseSagaMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SagaMiddleware>();
        }
    }
}
