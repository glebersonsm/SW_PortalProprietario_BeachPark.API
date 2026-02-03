using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.Saga;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using System.Text.Json;

namespace SW_PortalProprietario.Infra.Data.Repositories.Saga
{
    /// <summary>
    /// Repositório para persistir logs de Saga
    /// </summary>
    public class SagaRepository : ISagaRepository
    {
        private readonly IRepositoryHosted _repository;
        private readonly ILogger<SagaRepository> _logger;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public SagaRepository(
            IRepositoryHosted repository,
            ILogger<SagaRepository> logger,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SagaExecution> CreateSagaAsync(string operationType, string? inputData, string? metadata = null)
        {
            var saga = new SagaExecution
            {
                SagaId = Guid.NewGuid().ToString(),
                OperationType = operationType,
                Status = "Running",
                InputData = inputData,
                Metadata = metadata,
                DataHoraInicio = DateTime.Now,
                Endpoint = _httpContextAccessor?.HttpContext?.Request?.Path.Value,
                ClientIp = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString()
            };

            using (var session = _repository.CreateSession())
            {
                if (session == null) throw new InvalidOperationException("Não foi possível criar uma sessão de banco de dados.");
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        await session.InsertAsync(saga);
                        await transaction.CommitAsync();

                        _logger.LogInformation(
                            "Saga criada: {SagaId}, Tipo: {OperationType}",
                            saga.SagaId, operationType);

                        return saga;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Erro ao criar Saga: {OperationType}", operationType);
                        throw;
                    }
                }
            }
        }

        public async Task<SagaStep> AddStepAsync(string sagaId, string stepName, int order, bool canCompensate = true)
        {
            using (var session = _repository.CreateSession())
            {
                if (session == null) throw new InvalidOperationException("Não foi possível criar uma sessão de banco de dados.");
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var saga = await session.Query<SagaExecution>()
                            .Where(s => s.SagaId == sagaId)
                            .FirstOrDefaultAsync();

                        if (saga == null)
                            throw new InvalidOperationException($"Saga não encontrada: {sagaId}");

                        var step = new SagaStep
                        {
                            SagaExecutionId = saga.Id!.Value,
                            StepName = stepName,
                            StepOrder = order,
                            Status = "Pending",
                            PodeSerCompensado = canCompensate
                        };

                        await session.InsertAsync(step);
                        await transaction.CommitAsync();

                        _logger.LogDebug(
                            "Step adicionado: {StepName} (Ordem: {Order}) à Saga {SagaId}",
                            stepName, order, sagaId);

                        return step;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Erro ao adicionar step {StepName} à Saga {SagaId}", stepName, sagaId);
                        throw;
                    }
                }
            }
        }

        public async Task UpdateStepStatusAsync(int stepId, string status, string? outputData = null, string? errorMessage = null, string? stackTrace = null)
        {
            using (var session = _repository.CreateSession())
            {
                if (session == null) throw new InvalidOperationException("Não foi possível criar uma sessão de banco de dados.");
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var step = await session.GetAsync<SagaStep>(stepId);
                        if (step == null)
                            throw new InvalidOperationException($"Step não encontrado: {stepId}");

                        var now = DateTime.Now;

                        // Atualiza status e dados
                        step.Status = status;
                        step.OutputData = outputData;
                        step.ErrorMessage = errorMessage;
                        step.StackTrace = stackTrace;

                        // Controla timestamps baseado no status
                        switch (status)
                        {
                            case "Executing":
                                step.DataHoraInicio = now;
                                step.Tentativas++;
                                break;
                            case "Executed":
                            case "Failed":
                                step.DataHoraConclusao = now;
                                if (step.DataHoraInicio.HasValue)
                                    step.DuracaoMs = (long)(now - step.DataHoraInicio.Value).TotalMilliseconds;
                                break;
                            case "Compensating":
                                step.DataHoraInicioCompensacao = now;
                                step.TentativasCompensacao++;
                                break;
                            case "Compensated":
                                step.DataHoraConclusaoCompensacao = now;
                                if (step.DataHoraInicioCompensacao.HasValue)
                                    step.DuracaoCompensacaoMs = (long)(now - step.DataHoraInicioCompensacao.Value).TotalMilliseconds;
                                break;
                        }

                        await session.UpdateAsync(step);
                        await transaction.CommitAsync();

                        _logger.LogDebug(
                            "Step atualizado: {StepId}, Status: {Status}",
                            stepId, status);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Erro ao atualizar step {StepId}", stepId);
                        throw;
                    }
                }
            }
        }

        public async Task UpdateSagaStatusAsync(string sagaId, string status, string? outputData = null, string? errorMessage = null)
        {
            using (var session = _repository.CreateSession())
            {
                if (session == null) throw new InvalidOperationException("Não foi possível criar uma sessão de banco de dados.");
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var saga = await session.Query<SagaExecution>()
                            .Where(s => s.SagaId == sagaId)
                            .FirstOrDefaultAsync();

                        if (saga == null)
                            throw new InvalidOperationException($"Saga não encontrada: {sagaId}");

                        var now = DateTime.Now;
                        saga.Status = status;
                        saga.OutputData = outputData;
                        saga.ErrorMessage = errorMessage;
                        saga.DataHoraConclusao = now;
                        saga.DuracaoMs = (long)(now - saga.DataHoraInicio).TotalMilliseconds;

                        await session.UpdateAsync(saga);
                        await transaction.CommitAsync();

                        _logger.LogInformation(
                            "Saga atualizada: {SagaId}, Status: {Status}, Duração: {Duration}ms",
                            sagaId, status, saga.DuracaoMs);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Erro ao atualizar Saga {SagaId}", sagaId);
                        throw;
                    }
                }
            }
        }

        public async Task<SagaExecution?> GetSagaAsync(string sagaId)
        {
            using (var session = _repository.CreateSession())
            {
                if (session == null) return null;
                return await session.Query<SagaExecution>()
                    .Where(s => s.SagaId == sagaId)
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<IList<SagaStep>> GetStepsAsync(string sagaId)
        {
            using (var session = _repository.CreateSession())
            {
                if (session == null) return new List<SagaStep>();

                var saga = await session.Query<SagaExecution>()
                    .Where(s => s.SagaId == sagaId)
                    .FirstOrDefaultAsync();

                if (saga == null)
                    return new List<SagaStep>();

                return await session.Query<SagaStep>()
                    .Where(s => s.SagaExecutionId == saga.Id)
                    .OrderBy(s => s.StepOrder)
                    .ToListAsync();
            }
        }

        public async Task<IList<SagaExecution>> GetSagasByStatusAsync(string status, int limit = 100)
        {
            using (var session = _repository.CreateSession())
            {
                if (session == null) return new List<SagaExecution>();

                return await session.Query<SagaExecution>()
                    .Where(s => s.Status == status)
                    .OrderByDescending(s => s.DataHoraInicio)
                    .Take(limit)
                    .ToListAsync();
            }
        }

        public async Task<IList<SagaExecution>> GetSagasByOperationTypeAsync(string operationType, DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            using (var session = _repository.CreateSession())
            {
                if (session == null) return new List<SagaExecution>();

                var query = session.Query<SagaExecution>()
                    .Where(s => s.OperationType == operationType);

                if (dataInicio.HasValue)
                    query = query.Where(s => s.DataHoraInicio >= dataInicio.Value);

                if (dataFim.HasValue)
                    query = query.Where(s => s.DataHoraInicio <= dataFim.Value);

                return await query
                    .OrderByDescending(s => s.DataHoraInicio)
                    .ToListAsync();
            }
        }
    }
}
