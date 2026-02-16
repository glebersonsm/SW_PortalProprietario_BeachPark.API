using Dapper;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_Utils.Auxiliar;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class StateService : IStateService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<StateService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IServiceBase _serviceBase;
        public StateService(IRepositoryNH repository,
            ILogger<StateService> logger,
            IProjectObjectMapper mapper,
            IServiceBase serviceBase)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _serviceBase = serviceBase;
        }

        public async Task<DeleteResultModel> DeleteState(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            try
            {

                var state = await _repository.FindById<Estado>(id);
                if (state is null)
                {
                    throw new ArgumentException($"Não foi encontrado o estado com Id: {id}!");
                }


                _repository.BeginTransaction();
                await _repository.Remove(state);

                var resultCommit = await _repository.CommitAsync();
                if (resultCommit.executed)
                {
                    result.Result = "Removido com sucesso!";
                }
                else
                {
                    throw resultCommit.exception ?? new Exception("Não foi possível realizar a operação");
                }

                return result;

            }
            catch (Exception err)
            {
                _repository.Rollback();
                _logger.LogError(err, $"Não foi possível deletar o País: {id}");
                throw;
            }
        }

        public async Task<EstadoModel> SaveState(RegistroEstadoInputModel state)
        {
            try
            {
                _repository.BeginTransaction();
                var Stt = _mapper.Map(state, new Estado());

                var exist = (await _repository.FindByHql<Estado>($"From Estado e Inner Join Fetch e.Pais p Where (upper(e.Nome) = '{state.Nome?.TrimEnd().ToLower()}' and e.Pais = {state.PaisId.GetValueOrDefault()}) or e.CodigoIbge = '{state.CodigoIbge?.Trim()}'")).FirstOrDefault();
                if (exist != null)
                {
                    Stt = _mapper.Map(Stt, exist);
                }

                var result = await _repository.Save(Stt);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Estado: ({result.Id} - {state.CodigoIbge} - {state.Nome}) salvo com sucesso!");

                    if (result != null)
                        return _mapper.Map(result, new EstadoModel());

                }
                throw exception ?? new Exception($"Não foi possível salvar o Estado: ({state.CodigoIbge} - {state.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o Estado: ({state.CodigoIbge} - {state.Nome})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<EstadoModel>?> SearchState(EstadoSearchModel searchModel)
        {
            List<Parameter> parameters = new();
            List<Parameter> parameters1 = new();
            StringBuilder sb = new("From Estado ge Inner Join Fetch ge.Pais p Where 1 = 1");
            if (!string.IsNullOrEmpty(searchModel.Nome))
                sb.AppendLine($" and Lower(ge.Nome) like '%{searchModel.Nome.ToLower()}%'");

            if (!string.IsNullOrEmpty(searchModel.CodigoEstadoIbge))
            {
                sb.AppendLine($" and ge.CodigoIbge = :ibgestatecode");
                parameters.Add(new Parameter("ibgestatecode", searchModel.CodigoEstadoIbge));
            }

            var state = await _repository.FindByHql<Estado>(sb.ToString(), session: null, parameters.ToArray());

            if (state.Any())
                return await _serviceBase.SetUserName(state.Select(a => _mapper.Map<EstadoModel>(a)).AsList());

            return default;
        }

        public async Task<EstadoModel> UpdateState(AlteracaoEstadoInputModel model)
        {
            _repository.BeginTransaction();
            try
            {
                var state = (await _repository.FindByHql<Estado>("From Estado ge Inner Join Fetch ge.Pais p Where ge.Id = :id", session: null, new Parameter[]
                { new Parameter("id", model.Id) })).FirstOrDefault() ?? throw new Exception($"Não foi encontrado o estado: {model.Id}");

                state = _mapper.Map(model, state);

                await _repository.Save(state);
                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                    return _mapper.Map(state, new EstadoModel());
                else throw exception ?? new Exception("Erro na operação");
            }
            catch (Exception)
            {
                _repository.Rollback();
                throw;
            }
        }
    }
}
