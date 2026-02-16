using Dapper;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Functions;
using SW_Utils.Auxiliar;
using System.Net.Http.Json;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class CityService : ICityService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<CityService> _logger;
        private readonly IServiceBase _serviceBase;
        private readonly IProjectObjectMapper _mapper;
        private readonly ICommunicationProvider _communicationProvider;
        public CityService(IRepositoryNH repository,
            ILogger<CityService> logger,
            IServiceBase serviceBase,
            IProjectObjectMapper mapper,
            ICommunicationProvider communicationProvider)
        {
            _repository = repository;
            _logger = logger;
            _serviceBase = serviceBase;
            _mapper = mapper;
            _communicationProvider = communicationProvider;
        }

        public async Task<DeleteResultModel> DeleteCity(int id)
        {
            var result = new DeleteResultModel
            {
                Id = id
            };

            try
            {
                var city = await _repository.FindById<Cidade>(id) ?? throw new ArgumentException($"Não foi encontrado a cidade com Id: {id}!");
                _repository.BeginTransaction();
                await _repository.Remove(city);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    result.Result = "Removido com sucesso!";
                    _serviceBase.Compare(city, null);
                }
                else
                {
                    throw exception ?? new Exception("Não foi possível realizar a operação");
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

        public async Task<CidadeModel> SaveCity(RegistroCidadeInputModel city)
        {
            try
            {
                _repository.BeginTransaction();

                var exist = (await _repository.FindByHql<Cidade>($"From Cidade c Inner Join Fetch c.Estado e Inner Join Fetch e.Pais p Where (upper(c.Nome) = '{city.Nome?.TrimEnd().ToLower()}' and c.Estado = {city.EstadoId.GetValueOrDefault()}) or c.CodigoIbge = '{city.CodigoIbge}'")).FirstOrDefault();

                var citySave = _mapper.Map(city, new Cidade());
                if (exist != null)
                    citySave = _mapper.Map(citySave, exist);

                var result = await _repository.Save(citySave);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Cidade: ({result.Id} - {city.CodigoIbge} - {city.Nome}) salvo com sucesso!");
                    _serviceBase.Compare(citySave, exist);

                    if (result != null)
                        return _mapper.Map(result, new CidadeModel());
                }
                throw exception ?? new Exception("Erro na operação");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar a Cidade: ({city.CodigoIbge} - {city.Nome})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCity(CidadeSearchModel searchModel)
        {
            List<Parameter> parameters = new();
            List<Parameter> parameters1 = new();
            StringBuilder sb = new(@"Select 
                                      c.Id,
                                      c.Nome,
                                      c.CodigoIbge,
                                      e.Id as EstadoId,
                                      e.Nome as EstadoNome,
                                      e.CodigoIbge as EstadoCodigoIbge,
                                      e.Sigla as EstadoSigla,
                                      p.Id as PaisId,
                                      p.Nome as PaisNome,
                                      p.CodigoIbge as PaisCodigoIbge,
                                      c.Nome || '/' || e.Sigla as NomeFormatado
                                     From 
                                      Cidade c 
                                      Left Join Estado e on c.Estado = e.Id
                                      Left Join Pais p on e.Pais = p.Id 
                                     Where 
                                      1 = 1 ");

            if (_repository.DataBaseType == SW_Utils.Enum.EnumDataBaseType.SqlServer)
            {
                sb = new(@"Select 
                                      c.Id,
                                      c.Nome,
                                      c.CodigoIbge,
                                      e.Id as EstadoId,
                                      e.Nome as EstadoNome,
                                      e.CodigoIbge as EstadoCodigoIbge,
                                      e.Sigla as EstadoSigla,
                                      p.Id as PaisId,
                                      p.Nome as PaisNome,
                                      p.CodigoIbge as PaisCodigoIbge,
                                      c.Nome + '/' + e.Sigla as NomeFormatado
                                     From 
                                      Cidade c 
                                      Left Join Estado e on c.Estado = e.Id
                                      Left Join Pais p on e.Pais = p.Id 
                                     Where 
                                      1 = 1 ");
            }

            if (!string.IsNullOrEmpty(searchModel.Nome))
                sb.AppendLine($" and Lower(c.Nome) like '%{searchModel.Nome.ToLower().TrimEnd()}%'");

            if (!string.IsNullOrEmpty(searchModel.CodigoIbge))
            {
                sb.AppendLine($" and c.CodigoIbge like '%{searchModel.CodigoIbge.TrimEnd()}%'");
            }

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and c.Id = :idCidade");
                parameters.Add(new Parameter("idCidade", searchModel.Id));
            }

            if (!string.IsNullOrEmpty(searchModel.Search))
            {
                if (Helper.IsNumeric(searchModel.Search.Trim()))
                {
                    sb.AppendLine($" and c.Id = {searchModel.Id} ");
                }
                else
                {
                    var arrCidadeSigla = searchModel.Search.Split('/');
                    if (arrCidadeSigla.Length == 2)
                    {
                        sb.AppendLine($" and Lower(c.Nome) like '%{arrCidadeSigla[0].ToLower().Trim()}%' and Lower(e.Uf) like '%{arrCidadeSigla[1].ToLower().Trim()}%'");
                    }
                    else if (arrCidadeSigla.Length == 1)
                    {
                        sb.AppendLine($" and Lower(c.Nome) like '%{arrCidadeSigla[0].ToLower().TrimEnd()}%' ");
                    }
                }
            }

            var sql = sb.ToString();

            int totalRegistros = 0;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = 20;

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
            {
                totalRegistros = Convert.ToInt32(await _repository.CountTotalEntry(sql, session: null, parameters.ToArray()));
            }

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0 ||
                totalRegistros < (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault() * searchModel.NumeroDaPagina.GetValueOrDefault()) - searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1))
            {
                long totalPage = SW_Utils.Functions.Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(100), totalRegistros);
                if (totalPage < searchModel.NumeroDaPagina)
                    searchModel.NumeroDaPagina = Convert.ToInt32(totalPage);
            }

            sb.AppendLine(" Order by e.Id ");

            var cidades = searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0 ?
                await _repository.FindBySql<CidadeModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(1), parameters.ToArray())
                : await _repository.FindBySql<CidadeModel>(sb.ToString(), session: null, parameters.ToArray());

            if (cidades.Any())
            {
                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    Int64 totalPage = SW_Utils.Functions.Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);

                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPage), cidades.AsList());
                }

                return (1, 1, cidades.AsList());
            }

            return default;
        }

        public async Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCityOnProvider(CidadeSearchModel searchModel)
        {
            return await _communicationProvider.SearchCidade(searchModel);
        }

        public async Task<CidadeModel> UpdateCity(AlteracaoCidadeInputModel model)
        {
            _repository.BeginTransaction();
            try
            {
                var city = (await _repository.FindByHql<Cidade>($"From Cidade c Inner Join Fetch c.Estado e Inner Join Fetch e.Pais p Where c.Id = {model.Id}")).FirstOrDefault();
                if (city == null)
                    throw new ArgumentException($"Não foi encontrada a Cidade com Id: {model.Id.GetValueOrDefault(0)}");

                var objOld = await _serviceBase.GetObjectOld<Cidade>(model.Id.GetValueOrDefault());

                if (model.EstadoId.GetValueOrDefault(0) > 0)
                {
                    var estadoModel = (await _repository.FindByHql<Estado>($"From Estado e Inner Join Fetch e.Pais p Where e.Id = {model.EstadoId.GetValueOrDefault()}")).FirstOrDefault();
                    if (estadoModel == null)
                        throw new ArgumentException($"Não foi encontrado o Estado com Id: {model.EstadoId.GetValueOrDefault()}");

                    city.Estado = estadoModel;
                }

                city = _mapper.Map(model, city);

                await _repository.Save(city);
                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _serviceBase.Compare(objOld, city);
                    var result = await SearchCity(new CidadeSearchModel() { Id = city.Id, QuantidadeRegistrosRetornar = 1 });
                    if (result != null && result.Value.cidades.Any())
                        return result.Value.cidades.First();
                }

                throw exception ?? new Exception("Erro na operação");
            }
            catch (Exception)
            {
                _repository.Rollback();
                throw;
            }
        }

        public async Task<CepResponseModel> ConsultarCep(string cep)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cep))
                {
                    throw new ArgumentException("CEP é obrigatório");
                }

                var sanitizedCep = System.Text.RegularExpressions.Regex.Replace(cep, @"\D", "");
                if (sanitizedCep.Length != 8)
                {
                    throw new ArgumentException("CEP deve conter 8 dígitos");
                }

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var response = await client.GetAsync($"https://viacep.com.br/ws/{sanitizedCep}/json/");

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Não foi possível consultar o CEP: {sanitizedCep}. Status: {response.StatusCode}");
                        throw new Exception("Não foi possível consultar o CEP");
                    }

                    var cepData = await response.Content.ReadFromJsonAsync<CepResponseModel>();

                    if (cepData == null || cepData.erro == true)
                    {
                        _logger.LogWarning($"CEP não encontrado: {sanitizedCep}");
                        throw new ArgumentException("CEP não encontrado");
                    }

                    _logger.LogInformation($"CEP consultado com sucesso: {sanitizedCep}");
                    return cepData;
                }
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Erro ao consultar CEP: {cep}");
                throw new Exception($"Erro ao consultar CEP: {err.Message}", err);
            }
        }
    }
}
