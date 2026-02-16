using SW_Utils.Auxiliar;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using Dapper;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class RegraIntercambioService : IRegraIntercambioService
    {
        private readonly IRepositoryNH _repository;
        private readonly IRepositoryNHEsolPortal _repositoryPortal;
        private readonly ICacheStore _cacheStore;

        public RegraIntercambioService(
            IRepositoryNH repository,
            IRepositoryNHEsolPortal repositoryPortal,
            ICacheStore cacheStore)
        {
            _repository = repository;
            _repositoryPortal = repositoryPortal;
            _cacheStore = cacheStore;
        }

        public async Task<RegraIntercambioOpcoesModel> GetOpcoesAsync()
        {
            var cacheKey = "Opcoes_RegraIntercambio_";
            var cached = await _cacheStore.GetAsync<RegraIntercambioOpcoesModel>(cacheKey, 2, _repository.CancellationToken);
            if (cached != null)
                return cached;

            var result = new RegraIntercambioOpcoesModel();

            // 1. Tipos de semana eSolution: Média, Alta, Super Alta (e Baixa para compatibilidade)
            result.TiposSemanaESolution = new List<RegraIntercambioOpcaoItem>
            {
                new() { Value = "Média", Label = "Média" },
                new() { Value = "Alta", Label = "Alta" },
                new() { Value = "Super Alta", Label = "Super Alta" },
                new() { Value = "Baixa", Label = "Baixa" }
            };

            // 2. Tipos de semana CM: Super alta, Alta, Média, Baixa (FLGTIPO: S, A, M, B)
            result.TiposSemanaCM = new List<RegraIntercambioOpcaoItem>
            {
                new() { Value = "Super alta", Label = "Super alta" },
                new() { Value = "Alta", Label = "Alta" },
                new() { Value = "Média", Label = "Média" },
                new() { Value = "Baixa", Label = "Baixa" }
            };

            // 3. Tipos de contrato eSolution (Portal)
            try
            {
                var tiposContrato = (await _repositoryPortal.FindBySql<dynamic>(
                    "SELECT tc.Id, tc.Nome FROM TipoContrato tc WHERE (tc.UsuarioExclusao IS NULL OR tc.UsuarioExclusao = 0) AND (tc.DataHoraExclusao IS NULL)",
                    session: null)).AsList();
                result.TiposContrato = tiposContrato?
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
                result.TiposContrato = new List<RegraIntercambioOpcaoItem>();
            }

            await _cacheStore.AddAsync(cacheKey, result, DateTimeOffset.Now.AddMinutes(10), 2, _repository.CancellationToken);
            return result;
        }

        public async Task<List<RegraIntercambioModel>> GetAllAsync()
        {
            var configs = await _repository.FindByHql<RegraIntercambio>(
                "From RegraIntercambio r Order by r.Id");
            var tipoContratoLookup = await GetTipoContratoLookupAsync();
            return configs.Select(c => MapToModel(c, tipoContratoLookup)).ToList();
        }

        public async Task<RegraIntercambioModel?> GetByIdAsync(int id)
        {
            var configs = await _repository.FindByHql<RegraIntercambio>(
                "From RegraIntercambio r Where r.Id = :id", session: null, new Parameter("id", id));
            var config = configs.FirstOrDefault();
            if (config == null) return null;
            var tipoContratoLookup = await GetTipoContratoLookupAsync();
            return MapToModel(config, tipoContratoLookup);
        }

        public async Task<RegraIntercambioModel> CreateAsync(RegraIntercambioInputModel model)
        {
            ValidateInput(model);
            _repository.BeginTransaction();
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
                    DataHoraCriacao = DateTime.Now,
                    UsuarioCriacao = userId
                };
                await _repository.Save(entity);
                await _repository.CommitAsync();
                var lookup = await GetTipoContratoLookupAsync();
                return MapToModel(entity, lookup);
            }
            catch
            {
                _repository.Rollback();
                throw;
            }
        }

        public async Task<RegraIntercambioModel> UpdateAsync(int id, RegraIntercambioInputModel model)
        {
            ValidateInput(model);
            _repository.BeginTransaction();
            try
            {
                var configs = await _repository.FindByHql<RegraIntercambio>(
                    "From RegraIntercambio r Where r.Id = :id", session: null, new Parameter("id", id));
                var entity = configs.FirstOrDefault();
                if (entity == null)
                    throw new ArgumentException($"Regra de intercâmbio com ID {id} não encontrada");

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
                entity.DataHoraAlteracao = DateTime.Now;
                entity.UsuarioAlteracao = userId;

                await _repository.Save(entity);
                await _repository.CommitAsync();
                var lookup = await GetTipoContratoLookupAsync();
                return MapToModel(entity, lookup);
            }
            catch
            {
                _repository.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _repository.BeginTransaction();
            try
            {
                var configs = await _repository.FindByHql<RegraIntercambio>(
                    "From RegraIntercambio r Where r.Id = :id", session: null, new Parameter("id", id));
                var entity = configs.FirstOrDefault();
                if (entity == null)
                    throw new ArgumentException($"Regra de intercâmbio com ID {id} não encontrada");
                _repository.Remove(entity);
                await _repository.CommitAsync();
                return true;
            }
            catch
            {
                _repository.Rollback();
                throw;
            }
        }

        private async Task<Dictionary<int, string>> GetTipoContratoLookupAsync()
        {
            return new Dictionary<int, string>();
            //try
            //{
            //    var list = (await _repositoryPortal.FindBySql<dynamic>(
            //        "SELECT Id, Nome FROM TipoContrato WHERE (UsuarioExclusao IS NULL OR UsuarioExclusao = 0) AND (DataHoraExclusao IS NULL)",
            //        session: null)).AsList();
            //    return list?
            //        .Where(x => x.Id != null)
            //        .ToDictionary(x => Convert.ToInt32(x.Id), x => x.Nome?.ToString() ?? "") ?? new Dictionary<int, string>();
            //}
            //catch
            //{
            //    return new Dictionary<int, string>();
            //}
        }

        private static void ValidateInput(RegraIntercambioInputModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TipoSemanaCedida))
                throw new ArgumentException("Tipo de semana cedida deve ser informado");
            if (string.IsNullOrWhiteSpace(model.TiposSemanaPermitidosUso))
                throw new ArgumentException("Tipos de semana permitidos para uso devem ser informados");
            if (model.DataFimVigenciaCriacao < model.DataInicioVigenciaCriacao)
                throw new ArgumentException("Data fim da vigência de criação deve ser maior ou igual à data início");
            if (model.DataFimVigenciaUso < model.DataInicioVigenciaUso)
                throw new ArgumentException("Data fim da vigência de uso deve ser maior ou igual à data início");
        }

        private static RegraIntercambioModel MapToModel(RegraIntercambio e, Dictionary<int, string> tipoContratoLookup)
        {
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
                DataHoraCriacao = e.DataHoraCriacao,
                DataHoraAlteracao = e.DataHoraAlteracao
            };
        }
    }
}
