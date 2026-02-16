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
    public class CountryService : ICountryService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<CountryService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IServiceBase _serviceBase;
        public CountryService(IRepositoryNH repository,
            ILogger<CountryService> logger,
            IProjectObjectMapper mapper,
            IServiceBase serviceBase)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _serviceBase = serviceBase;
        }

        public async Task<DeleteResultModel> DeleteCountry(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            try
            {

                var country = await _repository.FindById<Pais>(id);
                if (country is null)
                {
                    throw new Exception($"Não foi encontrado o país com Id: {id}!");
                }


                _repository.BeginTransaction();
                await _repository.Remove(country);

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

        public async Task<PaisModel> SaveCountry(RegistroPaisInputModel country)
        {
            try
            {
                var ctry = _mapper.Map(country, new Pais());
                _repository.BeginTransaction();
                var exist = (await _repository.FindBySql<PaisModel>($"Select p.* From Pais p Where upper(p.Nome) = '{country.Nome?.TrimEnd().ToLower()}' or p.CodigoIbge = '{country.CodigoIbge.Trim()}'")).FirstOrDefault();
                if (exist != null)
                {

                    ctry.Id = exist.Id.GetValueOrDefault();

                }

                var result = await _repository.Save(ctry);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"País: ({result.Id} - {country.CodigoIbge} - {country.Nome}) salvo com sucesso!");

                    if (result != null)
                        return _mapper.Map(result, new PaisModel());

                }

                throw exception ?? new Exception($"Não foi possível salvar o País: ({country.CodigoIbge} - {country.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o País: ({country.CodigoIbge} - {country.Nome})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<PaisModel>?> SearchCountry(CountrySearchModel searchModel)
        {
            List<Parameter> parameters = new();
            List<Parameter> parameters1 = new();
            StringBuilder sb = new("From Pais ge Where 1 = 1");
            if (!string.IsNullOrEmpty(searchModel.Nome))
                sb.AppendLine($" and Lower(ge.Nome) like '%{searchModel.Nome.ToLower()}%'");

            if (!string.IsNullOrEmpty(searchModel.CodeIbgeCountry))
            {
                sb.AppendLine($" and ge.CodigoPaisIbge = :codeibgecountry");
                parameters.Add(new Parameter("codeibgecountry", searchModel.CodeIbgeCountry));
            }

            var country = await _repository.FindByHql<Pais>(sb.ToString(), session: null, parameters.ToArray());

            if (country.Any())
                return await _serviceBase.SetUserName(country.Select(a => _mapper.Map<PaisModel>(a)).AsList());

            return default;
        }

        public async Task<PaisModel> UpdateCountry(AlteracaoPaisInputModel model)
        {
            _repository.BeginTransaction();
            try
            {
                var country = (await _repository.FindByHql<Pais>("From Pais ge Where ge.Id = :id", session: null, new Parameter[] { new Parameter("id", model.Id) })).FirstOrDefault() ?? throw new Exception($"Não foi encontrado o país: {model.Id}");
                country = _mapper.Map(model, country);
                await _repository.Save(country);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                    return _mapper.Map(country, new PaisModel());
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
