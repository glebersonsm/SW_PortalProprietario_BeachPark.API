using Dapper;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_Utils.Auxiliar;
using ZXing;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class RegraIntercambioService : IRegraIntercambioService
    {
        private readonly IRepositoryNH _repository;
        private readonly IRepositoryNHEsolPortal _repositoryPortal;
        private readonly IRepositoryNHCm _repositoryCM;
        private readonly ICacheStore _cacheStore;
        private ILogger<RegraIntercambioService> _logger;

        public RegraIntercambioService(
            IRepositoryNH repository,
            IRepositoryNHCm repositoryCM,
            IRepositoryNHEsolPortal repositoryPortal,
            ICacheStore cacheStore,
            ILogger<RegraIntercambioService> logger)
        {
            _repository = repository;
            _repositoryPortal = repositoryPortal;
            _cacheStore = cacheStore;
            _repositoryCM = repositoryCM;
            _logger = logger;
        }

        public async Task<RegraIntercambioOpcoesModel> GetOpcoesAsync()
        {
            var cacheKey = "Opcoes_RegraIntercambio_";
            var cached = await _cacheStore.GetAsync<RegraIntercambioOpcoesModel>(cacheKey, 2, _repository.CancellationToken);
            if (cached != null)
                return cached;

            var result = new RegraIntercambioOpcoesModel();

            // 1. Tipos de semana eSolution: Média, Alta, Super Alta (e Baixa para compatibilidade)
            try
            {
                result.TiposSemanaESolution = (await _repositoryPortal.FindBySql<TipoSemanaModel>(
                    "SELECT ts.Id, ts.Empresa, ts.Nome FROM TipoSemana ts WHERE ts.UsuarioExclusao is null and ts.DataHoraExclusao is null",
                    session: null)).AsList();
            }
            catch
            {
                result.TiposSemanaESolution = new List<TipoSemanaModel>();
            }

            // 2. Tipos de semana CM: Super alta, Alta, Média, Baixa (FLGTIPO: S, A, M, B)
            try
            {
                result.TiposSemanaCM = (await _repositoryCM.FindBySql<TipoSemanaModel>(
                    @"SELECT 
                        ts.IdTemporadaTs AS Id,
                        ts.Descricao AS Nome,
                        Decode(ts.FlgTipo,'B','BAIXA','S','SUPER ALTA','M','MÉDIA','A','ALTA') AS Complemento
                        FROM TEMPORADATS ts
                        WHERE 1 = 1",
                    session: null)).AsList();
            }
            catch
            {
                result.TiposSemanaCM = new List<TipoSemanaModel>();
            }

            // 3. Tipos de contrato eSolution (Portal)
            try
            {
                result.TiposContrato = await GetTiposContratoAsync();
            }
            catch
            {
                result.TiposContrato = new List<RegraIntercambioOpcaoItem>();
            }

            // 4. Tipos de UH (TipoUh - eSolution/CM)
            try
            {
                result.TiposUh = await GetTiposUhAsync();
            }
            catch
            {
                result.TiposUh = new List<RegraIntercambioOpcaoItem>();
            }

            await _cacheStore.AddAsync(cacheKey, result, DateTimeOffset.Now.AddMinutes(10), 2, _repository.CancellationToken);
            return result;
        }

        public async Task<List<RegraIntercambioModel>> GetAllAsync()
        {
            var configs = await _repository.FindByHql<RegraIntercambio>(
                "From RegraIntercambio r Order by r.Id");
            var tipoContratoLookup = await GetTipoContratoLookupAsync();
            var tipoUhLookup = await GetTipoUhLookupAsync();
            return configs.Select(c => MapToModel(c, tipoContratoLookup, tipoUhLookup)).ToList();
        }

        public async Task<RegraIntercambioModel?> GetByIdAsync(int id)
        {
            var configs = await _repository.FindByHql<RegraIntercambio>(
                "From RegraIntercambio r Where r.Id = :id", session: null, new Parameter("id", id));
            var config = configs.FirstOrDefault();
            if (config == null) return null;
            var tipoContratoLookup = await GetTipoContratoLookupAsync();
            var tipoUhLookup = await GetTipoUhLookupAsync();
            return MapToModel(config, tipoContratoLookup, tipoUhLookup);
        }

        public async Task<RegraIntercambioModel> CreateAsync(RegraIntercambioInputModel model)
        {
            ValidateInput(model);
            _repository.BeginTransaction();
            var committed = false;
            try
            {
                var loggedUser = await _repository.GetLoggedUser();
                var userId = (loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId) && int.TryParse(loggedUser.Value.userId, out var uid))
                    ? (int?)uid : null;
                var entity = new RegraIntercambio
                {
                    TipoContratoId = model.TipoContratoId,
                    TipoSemanaCedida = model.TipoSemanaCedida ?? string.Empty,
                    TiposSemanaPermitidosUso = model.TiposSemanaPermitidosUso ?? string.Empty,
                    DataInicioVigenciaCriacao = model.DataInicioVigenciaCriacao,
                    DataFimVigenciaCriacao = model.DataFimVigenciaCriacao,
                    DataInicioVigenciaUso = model.DataInicioVigenciaUso,
                    DataFimVigenciaUso = model.DataFimVigenciaUso,
                    TiposUhIds = string.IsNullOrWhiteSpace(model.TiposUhIds) ? null : model.TiposUhIds.Trim(),
                    DataHoraCriacao = DateTime.Now,
                    UsuarioCriacao = userId
                };
                await _repository.Save(entity);
                var (executed, exception) = await _repository.CommitAsync();
                if (!executed)
                    throw exception ?? new Exception("Operação não realizada.");
                committed = true;
                var lookup = await GetTipoContratoLookupAsync();
                var tipoUhLookup = await GetTipoUhLookupAsync();
                return MapToModel(entity, lookup, tipoUhLookup);
            }
            finally
            {
                if (!committed)
                    _repository.Rollback();
            }
        }

        public async Task<RegraIntercambioModel> UpdateAsync(RegraIntercambioInputModel model)
        {
            ValidateInput(model);
            _repository.BeginTransaction();
            var committed = false;
            try
            {
                var configs = (await _repository.FindByHql<RegraIntercambio>(
                    "From RegraIntercambio r Where r.Id = :id", session: null, new Parameter("id", model.Id))).ToList();
                var entity = configs.FirstOrDefault();
                if (entity == null)
                    throw new ArgumentException($"Regra de intercâmbio com ID {model.Id} não encontrada");

                var loggedUser = await _repository.GetLoggedUser();
                var userId = (loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId) && int.TryParse(loggedUser.Value.userId, out var uid))
                    ? (int?)uid : null;

                entity.TipoContratoId = model.TipoContratoId;
                entity.TipoSemanaCedida = model.TipoSemanaCedida ?? string.Empty;
                entity.TiposSemanaPermitidosUso = model.TiposSemanaPermitidosUso ?? string.Empty;
                entity.DataInicioVigenciaCriacao = model.DataInicioVigenciaCriacao;
                entity.DataFimVigenciaCriacao = model.DataFimVigenciaCriacao;
                entity.DataInicioVigenciaUso = model.DataInicioVigenciaUso;
                entity.DataFimVigenciaUso = model.DataFimVigenciaUso;
                entity.TiposUhIds = string.IsNullOrWhiteSpace(model.TiposUhIds) ? null : model.TiposUhIds.Trim();
                entity.DataHoraAlteracao = DateTime.Now;
                entity.UsuarioAlteracao = userId;

                await _repository.Save(entity);
                var (executed, exception) = await _repository.CommitAsync();
                if (!executed)
                    throw exception ?? new Exception("Operação não realizada.");
                committed = true;
                var lookup = await GetTipoContratoLookupAsync();
                var tipoUhLookup = await GetTipoUhLookupAsync();
                return MapToModel(entity, lookup, tipoUhLookup);
            }
            finally
            {
                if (!committed)
                    _repository.Rollback();
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _repository.BeginTransaction();
            var committed = false;
            try
            {
                var configs = (await _repository.FindByHql<RegraIntercambio>(
                    "From RegraIntercambio r Where r.Id = :id", session: null, new Parameter("id", id))).ToList();
                var entity = configs.FirstOrDefault();
                if (entity == null)
                    throw new ArgumentException($"Regra de intercâmbio com ID {id} não encontrada");

                await _repository.Remove(entity);
                var (executed, exception) = await _repository.CommitAsync();
                if (!executed)
                    throw exception ?? new Exception("Operação não realizada.");
                committed = true;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar regra de intercâmbio com ID {Id}", id);
                throw;
            }
            finally
            {
                if (!committed)
                    _repository.Rollback();
            }
        }

        private async Task<Dictionary<int, string>> GetTipoContratoLookupAsync()
        {
            try
            {
                var tiposContrato = await GetTiposContratoAsync();
                
                if (tiposContrato == null || !tiposContrato.Any())
                    return new Dictionary<int, string>();

                return tiposContrato
                    .Where(x => x.Id.HasValue)
                    .ToDictionary(x => x.Id!.Value, x => x.Label ?? "");
            }
            catch
            {
                return new Dictionary<int, string>();
            }
        }

        private async Task<List<RegraIntercambioOpcaoItem>> GetTiposContratoAsync()
        {
            try
            {
                var tiposContrato = (await _repositoryPortal.FindBySql<dynamic>(
                    "SELECT tc.Id, tc.Nome FROM TipoContrato tc WHERE (tc.UsuarioExclusao IS NULL OR tc.UsuarioExclusao = 0) AND (tc.DataHoraExclusao IS NULL)",
                    session: null)).AsList();

                return tiposContrato?
                    .Select(x => new RegraIntercambioOpcaoItem
                    {
                        Value = x.Id?.ToString() ?? "",
                        Label = x.Nome?.ToString() ?? x.Id?.ToString() ?? "",
                        Id = x.Id != null ? (int?)Convert.ToInt32(x.Id) : null
                    })
                    .Where(x => !string.IsNullOrEmpty(x.Value))
                    .ToList() ?? new List<RegraIntercambioOpcaoItem>();
            }
            catch
            {
                return new List<RegraIntercambioOpcaoItem>();
            }
        }

        private async Task<Dictionary<int, string>> GetTipoUhLookupAsync()
        {
            try
            {
                var tiposUh = await GetTiposUhAsync();
                if (tiposUh == null || !tiposUh.Any())
                    return new Dictionary<int, string>();
                return tiposUh
                    .Where(x => x.Id.HasValue)
                    .ToDictionary(x => x.Id!.Value, x => x.Label ?? "");
            }
            catch
            {
                return new Dictionary<int, string>();
            }
        }

        private async Task<List<RegraIntercambioOpcaoItem>> GetTiposUhAsync()
        {
            try
            {
                var tiposUh = (await _repositoryCM.FindBySql<TipoUhModel>(
                    @"SELECT t.IdTipoUh, t.IdHotel, t.CodReduzido, t.Descricao 
                      FROM TipoUh t 
                      ORDER BY t.Descricao",
                    session: null)).AsList();

                return tiposUh?
                    .Where(x => x.IdTipoUh.HasValue)
                    .Select(x => new RegraIntercambioOpcaoItem
                    {
                        Value = x.IdTipoUh!.Value.ToString(),
                        Label = $"{x.CodReduzido ?? ""} - {x.Descricao ?? ""}".Trim(' ', '-').Trim() 
                            ?? x.IdTipoUh.Value.ToString(),
                        Id = x.IdTipoUh
                    })
                    .ToList() ?? new List<RegraIntercambioOpcaoItem>();
            }
            catch
            {
                return new List<RegraIntercambioOpcaoItem>();
            }
        }

        private static void ValidateInput(RegraIntercambioInputModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TipoSemanaCedida))
                throw new ArgumentException("Tipo de semana cedida (eSolution) deve ser informado");
            if (string.IsNullOrWhiteSpace(model.TiposSemanaPermitidosUso))
                throw new ArgumentException("Tipos de semana permitidos para uso (CM) devem ser informados");
            if (model.DataFimVigenciaCriacao.HasValue && model.DataFimVigenciaCriacao.Value < model.DataInicioVigenciaCriacao)
                throw new ArgumentException("Data fim da vigência de criação deve ser maior ou igual à data início");
            if (model.DataFimVigenciaUso.HasValue && model.DataFimVigenciaUso.Value < model.DataInicioVigenciaUso)
                throw new ArgumentException("Data fim da vigência de uso deve ser maior ou igual à data início");
        }

        private static RegraIntercambioModel MapToModel(RegraIntercambio e, Dictionary<int, string> tipoContratoLookup, Dictionary<int, string> tipoUhLookup)
        {
            var tiposUhNomes = string.IsNullOrWhiteSpace(e.TiposUhIds)
                ? null
                : string.Join(", ", e.TiposUhIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => int.TryParse(s, out var id) && tipoUhLookup.TryGetValue(id, out var n) ? n : s)
                    .Where(x => !string.IsNullOrEmpty(x)));

            return new RegraIntercambioModel
            {
                Id = e.Id,
                TipoContratoId = e.TipoContratoId,
                TipoContratoNome = e.TipoContratoId.HasValue && tipoContratoLookup.TryGetValue(e.TipoContratoId.Value, out var nome)
                    ? nome : (e.TipoContratoId == null ? "Todos" : null),
                TipoSemanaCedida = e.TipoSemanaCedida,
                TiposSemanaPermitidosUso = e.TiposSemanaPermitidosUso,
                DataInicioVigenciaCriacao = e.DataInicioVigenciaCriacao,
                DataFimVigenciaCriacao = e.DataFimVigenciaCriacao,
                DataInicioVigenciaUso = e.DataInicioVigenciaUso,
                DataFimVigenciaUso = e.DataFimVigenciaUso,
                TiposUhIds = e.TiposUhIds,
                TiposUhNomes = tiposUhNomes,
                DataHoraCriacao = e.DataHoraCriacao,
                DataHoraAlteracao = e.DataHoraAlteracao
            };
        }
    }
}
