using Dapper;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_Utils.Auxiliar;
using System.Text;
using System.Linq;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class RegraPaxFreeService : IRegraPaxFreeService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<RegraPaxFreeService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IServiceBase _serviceBase;

        public RegraPaxFreeService(
            IRepositoryNH repository,
            ILogger<RegraPaxFreeService> logger,
            IProjectObjectMapper mapper,
            IServiceBase serviceBase)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _serviceBase = serviceBase;
        }

        public async Task<RegraPaxFreeModel> SaveRegraPaxFree(RegraPaxFreeInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                if (string.IsNullOrEmpty(model.Nome))
                    throw new ArgumentException("Deve ser informado o nome da regra");

                RegraPaxFree regra = null;
                if (model.Id.GetValueOrDefault(0) > 0)
                {
                    regra = (await _repository.FindByHql<RegraPaxFree>($"From RegraPaxFree r Where r.Id = {model.Id}")).FirstOrDefault();
                }

                if (regra == null)
                {
                    regra = new RegraPaxFree();
                }

                regra.Nome = model.Nome;
                regra.DataInicioVigencia = model.DataInicioVigencia;
                regra.DataFimVigencia = model.DataFimVigencia;

                var result = await _repository.Save(regra);
                await SincronizarConfiguracoes(result, model.Configuracoes ?? new List<RegraPaxFreeConfiguracaoInputModel>(), model.RemoverConfiguracoesNaoEnviadas.GetValueOrDefault(false));
                await SincronizarHoteis(result, model.Hoteis ?? new List<RegraPaxFreeHotelInputModel>(), model.RemoverHoteisNaoEnviados.GetValueOrDefault(false));

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"RegraPaxFree: ({result.Id} - {regra.Nome}) salva com sucesso!");

                    if (result != null)
                    {
                        var searchResult = new List<RegraPaxFreeModel>() { new RegraPaxFreeModel() { Id = result.Id, Nome = regra.Nome, DataInicioVigencia = regra.DataInicioVigencia, DataFimVigencia = regra.DataFimVigencia,
                            Hoteis = model.Hoteis != null && model.Hoteis.Any() ? model.Hoteis.Select(a=> new RegraPaxFreeHotelModel()
                            {
                                Id = a.Id,
                                HotelId = a.HotelId
                            }).AsList() : new List<RegraPaxFreeHotelModel>() } };
                        if (searchResult != null && searchResult.Any())
                            return searchResult.First();
                    }
                }
                throw exception ?? new Exception($"Não foi possível salvar a RegraPaxFree: ({regra.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar a RegraPaxFree: ({model.Nome})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<RegraPaxFreeModel> UpdateRegraPaxFree(AlteracaoRegraPaxFreeInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                if (model.Id.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser informado Id da regra.");

                RegraPaxFree regra = (await _repository.FindByHql<RegraPaxFree>($"From RegraPaxFree r Where r.UsuarioRemocao is null and r.DataHoraRemocao is null and r.Id = {model.Id}")).FirstOrDefault();
                if (regra == null)
                    throw new ArgumentException($"Não foi encontrada a regra com Id: {model.Id}");

                regra.Nome = model.Nome;
                regra.DataInicioVigencia = model.DataInicioVigencia;
                regra.DataFimVigencia = model.DataFimVigencia;

                var result = await _repository.Save(regra);
                await SincronizarConfiguracoes(result, model.Configuracoes ?? new List<RegraPaxFreeConfiguracaoInputModel>(), model.RemoverConfiguracoesNaoEnviadas.GetValueOrDefault(false));
                await SincronizarHoteis(result, model.Hoteis ?? new List<RegraPaxFreeHotelInputModel>(), model.RemoverHoteisNaoEnviados.GetValueOrDefault(false));

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"RegraPaxFree: ({result.Id} - {regra.Nome}) salva com sucesso!");

                    if (result != null)
                    {
                        var searchResult = new List<RegraPaxFreeModel>() { new RegraPaxFreeModel() { Id = result.Id, Nome = regra.Nome, DataInicioVigencia = regra.DataInicioVigencia, DataFimVigencia = regra.DataFimVigencia,
                            Hoteis = model.Hoteis != null && model.Hoteis.Any() ? model.Hoteis.Select(a=> new RegraPaxFreeHotelModel()
                            {
                                Id = a.Id,
                                HotelId = a.HotelId
                            }).AsList() : new List<RegraPaxFreeHotelModel>() } };
                        if (searchResult != null && searchResult.Any())
                            return searchResult.First();
                    }
                    else 
                    {
                        throw new Exception($"Não foi possível salvar a RegraPaxFree: ({regra.Nome})");
                    }
                }
                throw exception ?? new Exception($"Não foi possível salvar a RegraPaxFree: ({regra.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar a RegraPaxFree: ({model.Nome})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<DeleteResultModel> DeleteRegraPaxFree(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            try
            {
                var loggedUser = (await _repository.GetLoggedUser());

                var regra = (await _repository.FindByHql<RegraPaxFree>($"From RegraPaxFree r Where r.DataHoraRemocao is null and r.UsuarioRemocao is null and r.Id = {id}")).FirstOrDefault();
                if (regra is null)
                {
                    throw new ArgumentException($"Não foi encontrada a regra com Id: {id}!");
                }

                _repository.BeginTransaction();

                // Remover configurações
                var configuracoes = (await _repository.FindByHql<RegraPaxFreeConfiguracao>($"From RegraPaxFreeConfiguracao rpc Where rpc.RegraPaxFree.Id = {regra.Id}")).AsList();
                foreach (var config in configuracoes)
                {
                    config.UsuarioRemocao = loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId) ? Convert.ToInt32(loggedUser.Value.userId) : null;
                    config.DataHoraRemocao = DateTime.Now;
                    await _repository.Save(config);
                }

                regra.DataHoraRemocao = DateTime.Now;
                regra.UsuarioRemocao = loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId) ? Convert.ToInt32(loggedUser.Value.userId) : null;

                await _repository.Save(regra);

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
                _logger.LogError(err, $"Não foi possível deletar a regra: {id}");
                throw;
            }
        }

        public async Task<IEnumerable<RegraPaxFreeModel>?> Search(SearchPadraoModel searchModel)
        {
            List<Parameter> parameters = new();
            StringBuilder sb = new("From RegraPaxFree r Where r.DataHoraRemocao is null and r.UsuarioRemocao is null ");

            if (!string.IsNullOrEmpty(searchModel.Nome))
                sb.AppendLine($" and Lower(r.Nome) like '%{searchModel.Nome.ToLower()}%'");

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and r.Id = :id");
                parameters.Add(new Parameter("id", searchModel.Id.GetValueOrDefault()));
            }

            if (searchModel.Ids != null && searchModel.Ids.Any())
            {
                sb.AppendLine($" and r.Id in ({string.Join(",", searchModel.Ids.AsList())})");
            }

            if (searchModel.UsuarioCriacao.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and r.UsuarioCriacao = {searchModel.UsuarioCriacao.GetValueOrDefault()}");
            }

            var regras = await _repository.FindByHql<RegraPaxFree>(sb.ToString(), session: null, parameters.ToArray());

            var listRegrasRetorno = regras.Select(a => _mapper.Map(a, new RegraPaxFreeModel())).ToList();

            if (listRegrasRetorno != null && listRegrasRetorno.Any())
            {
                var configuracoes = (await _repository.FindByHql<RegraPaxFreeConfiguracao>($"From RegraPaxFreeConfiguracao rpc Inner Join Fetch rpc.RegraPaxFree r Where rpc.UsuarioRemocao is null and rpc.DataHoraRemocao is null and r.Id in ({string.Join(",", listRegrasRetorno.Select(a => a.Id).AsList())})", session: null)).AsList();
                foreach (var item in listRegrasRetorno)
                {
                    var configsRelacionadas = configuracoes.Where(b => b.RegraPaxFree.Id == item.Id).AsList();
                    if (configsRelacionadas.Any())
                    {
                        item.Configuracoes = configsRelacionadas.Select(b => new RegraPaxFreeConfiguracaoModel()
                        {
                            Id = b.Id,
                            RegraPaxFreeId = b.RegraPaxFree.Id,
                            QuantidadeAdultos = b.QuantidadeAdultos,
                            QuantidadePessoasFree = b.QuantidadePessoasFree,
                            IdadeMaximaAnos = b.IdadeMaximaAnos,
                            TipoOperadorIdade = b.TipoOperadorIdade ?? "<=", // Valor padrão para compatibilidade
                            TipoDataReferencia = b.TipoDataReferencia ?? "RESERVA" // Valor padrão para compatibilidade
                        }).AsList();
                    }

                    // Buscar hotéis vinculados
                    var hoteis = (await _repository.FindByHql<RegraPaxFreeHotel>($"From RegraPaxFreeHotel rph Inner Join Fetch rph.RegraPaxFree r Where rph.UsuarioRemocao is null and rph.DataHoraRemocao is null and r.Id = {item.Id}")).AsList();
                    if (hoteis != null && hoteis.Any())
                    {
                        item.Hoteis = hoteis.Select(h => new RegraPaxFreeHotelModel()
                        {
                            Id = h.Id,
                            RegraPaxFreeId = h.RegraPaxFree?.Id,
                            HotelId = h.HotelId
                        }).AsList();
                    }
                }
            }

            return await _serviceBase.SetUserName(listRegrasRetorno);
        }

        public async Task<RegraPaxFreeModel?> GetRegraVigente(int? hotelId = null)
        {

            RegraPaxFreeModel? regraResult = null;
            List<Parameter> parameters = new List<Parameter>() { new Parameter("dataAtual", DateTime.Today) };
            StringBuilder sb = new(@$"From 
                                        RegraPaxFree r 
                                      Where 
                                        r.DataHoraRemocao is null and 
                                        r.UsuarioRemocao is null and 
                                        r.DataInicioVigencia is not null and 
                                        r.DataInicioVigencia <= :dataAtual and 
                                        (r.DataFimVigencia is null or r.DataFimVigencia >= :dataAtual)");
            
            // Se hotelId foi informado, filtrar regras que:
            // - Não têm hotéis vinculados (se aplicam a todos) OU
            // - Têm o hotelId específico vinculado
            if (hotelId.HasValue && hotelId.Value > 0)
            {
                sb.Append(@" and (
                                    not exists (
                                        Select 1 From RegraPaxFreeHotel rph 
                                        Where rph.RegraPaxFree.Id = r.Id and 
                                              rph.UsuarioRemocao is null and 
                                              rph.DataHoraRemocao is null
                                    ) or 
                                    exists (
                                        Select 1 From RegraPaxFreeHotel rph 
                                        Where rph.RegraPaxFree.Id = r.Id and 
                                              rph.HotelId = ").Append(hotelId.Value).Append(@" and 
                                              rph.UsuarioRemocao is null and 
                                              rph.DataHoraRemocao is null
                                    )
                                )");
            }
            
            sb.Append(" Order by r.Id Desc");

            


            var regra = (await _repository.FindByHql<RegraPaxFree>(sb.ToString(), session: null, parameters.ToArray())).FirstOrDefault();

            if (regra != null)
            {
                regraResult = _mapper.Map(regra, new RegraPaxFreeModel());

                if (regraResult != null)
                {
                    var configuracoes = (await _repository.FindByHql<RegraPaxFreeConfiguracao>($"From RegraPaxFreeConfiguracao rpc Inner Join Fetch rpc.RegraPaxFree r Where rpc.UsuarioRemocao is null and rpc.DataHoraRemocao is null and r.Id in ({regraResult.Id})")).AsList();
                    if (configuracoes != null && configuracoes.Any())
                    {
                        regraResult.Configuracoes = configuracoes.Select(b => new RegraPaxFreeConfiguracaoModel()
                        {
                            Id = b.Id,
                            RegraPaxFreeId = b.RegraPaxFree?.Id,
                            QuantidadeAdultos = b.QuantidadeAdultos,
                            QuantidadePessoasFree = b.QuantidadePessoasFree,
                            IdadeMaximaAnos = b.IdadeMaximaAnos,
                            TipoOperadorIdade = b.TipoOperadorIdade ?? "<=", // Valor padrão para compatibilidade
                            TipoDataReferencia = b.TipoDataReferencia ?? "RESERVA" // Valor padrão para compatibilidade
                        }).AsList();
                    }

                    // Buscar hotéis vinculados
                    var hoteis = (await _repository.FindByHql<RegraPaxFreeHotel>($"From RegraPaxFreeHotel rph Inner Join Fetch rph.RegraPaxFree r Where rph.UsuarioRemocao is null and rph.DataHoraRemocao is null and r.Id = {regraResult.Id}")).AsList();
                    if (hoteis != null && hoteis.Any())
                    {
                        regraResult.Hoteis = hoteis.Select(h => new RegraPaxFreeHotelModel()
                        {
                            Id = h.Id,
                            RegraPaxFreeId = h.RegraPaxFree?.Id,
                            HotelId = h.HotelId
                        }).AsList();
                    }
                }
            }

            return regraResult;
        }

        private async Task SincronizarConfiguracoes(RegraPaxFree regra, List<RegraPaxFreeConfiguracaoInputModel> listConfigs, bool removerConfigsNaoEnviadas = false)
        {
            if (removerConfigsNaoEnviadas)
            {
                if (listConfigs == null || listConfigs.Count == 0)
                {
                    await _repository.ExecuteSqlCommand($"Delete From RegraPaxFreeConfiguracao Where RegraPaxFree = {regra.Id}");
                    return;
                }
                else
                {
                    var idsManter = string.Join(",", listConfigs.Where(c => c.Id.HasValue && c.Id.Value > 0).Select(c => c.Id.Value));
                    if (!string.IsNullOrEmpty(idsManter))
                    {
                        await _repository.ExecuteSqlCommand($"Delete From RegraPaxFreeConfiguracao Where RegraPaxFree = {regra.Id} and Id not in ({idsManter})");
                    }
                    else
                    {
                        await _repository.ExecuteSqlCommand($"Delete From RegraPaxFreeConfiguracao Where RegraPaxFree = {regra.Id}");
                    }
                }
            }

            if (listConfigs != null && listConfigs.Any())
            {
                foreach (var configInput in listConfigs)
                {
                    RegraPaxFreeConfiguracao config = null;
                    if (configInput.Id.HasValue && configInput.Id.Value > 0)
                    {
                        config = (await _repository.FindByHql<RegraPaxFreeConfiguracao>($"From RegraPaxFreeConfiguracao rpc Where rpc.Id = {configInput.Id.Value} and rpc.RegraPaxFree.Id = {regra.Id}")).FirstOrDefault();
                    }

                    if (config == null)
                    {
                        config = new RegraPaxFreeConfiguracao();
                        config.RegraPaxFree = regra;
                    }

                    config.QuantidadeAdultos = configInput.QuantidadeAdultos;
                    config.QuantidadePessoasFree = configInput.QuantidadePessoasFree;
                    config.IdadeMaximaAnos = configInput.IdadeMaximaAnos;
                    config.TipoOperadorIdade = string.IsNullOrEmpty(configInput.TipoOperadorIdade) ? "<=" : configInput.TipoOperadorIdade; // Valor padrão "<=" para compatibilidade
                    config.TipoDataReferencia = string.IsNullOrEmpty(configInput.TipoDataReferencia) ? "RESERVA" : configInput.TipoDataReferencia; // Valor padrão "RESERVA" para compatibilidade

                    await _repository.Save(config);
                }
            }
        }

        private async Task SincronizarHoteis(RegraPaxFree regra, List<RegraPaxFreeHotelInputModel> listHoteis, bool removerHoteisNaoEnviados = false)
        {
            var loggedUser = await _repository.GetLoggedUser();
            
            if (removerHoteisNaoEnviados)
            {
                if (listHoteis == null || listHoteis.Count == 0)
                {
                    var hoteisParaRemover = (await _repository.FindByHql<RegraPaxFreeHotel>($"From RegraPaxFreeHotel rph Where rph.UsuarioRemocao is null and rph.DataHoraRemocao is null and rph.RegraPaxFree.Id = {regra.Id}")).AsList();
                    foreach (var hotelParaRemover in hoteisParaRemover)
                    {
                        hotelParaRemover.UsuarioRemocao = loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId) ? Convert.ToInt32(loggedUser.Value.userId) : null;
                        hotelParaRemover.DataHoraRemocao = DateTime.Now;
                        await _repository.Save(hotelParaRemover);
                    }
                    return;
                }
                else
                {
                    var idsManter = listHoteis.Where(h => h.Id.HasValue && h.Id.Value > 0).Select(h => h.Id.Value).ToList();
                    if (idsManter.Any())
                    {
                        var hoteisParaRemover = (await _repository.FindByHql<RegraPaxFreeHotel>($"From RegraPaxFreeHotel rph Where rph.UsuarioRemocao is null and rph.DataHoraRemocao is null and rph.RegraPaxFree.Id = {regra.Id} and rph.Id not in ({string.Join(",", idsManter)})")).AsList();
                        foreach (var hotelParaRemover in hoteisParaRemover)
                        {
                            hotelParaRemover.UsuarioRemocao = loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId) ? Convert.ToInt32(loggedUser.Value.userId) : null;
                            hotelParaRemover.DataHoraRemocao = DateTime.Now;
                            await _repository.Save(hotelParaRemover);
                        }
                    }
                    else
                    {
                        var hoteisParaRemover = (await _repository.FindByHql<RegraPaxFreeHotel>($"From RegraPaxFreeHotel rph Where rph.UsuarioRemocao is null and rph.DataHoraRemocao is null and rph.RegraPaxFree.Id = {regra.Id}")).AsList();
                        foreach (var hotelParaRemover in hoteisParaRemover)
                        {
                            hotelParaRemover.UsuarioRemocao = loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId) ? Convert.ToInt32(loggedUser.Value.userId) : null;
                            hotelParaRemover.DataHoraRemocao = DateTime.Now;
                            await _repository.Save(hotelParaRemover);
                        }
                    }
                }
            }

            if (listHoteis != null && listHoteis.Any())
            {
                foreach (var hotelInput in listHoteis)
                {
                    if (hotelInput.HotelId == null || hotelInput.HotelId <= 0)
                        continue;

                    RegraPaxFreeHotel hotel = null;
                    if (hotelInput.Id.HasValue && hotelInput.Id.Value > 0)
                    {
                        hotel = (await _repository.FindByHql<RegraPaxFreeHotel>($"From RegraPaxFreeHotel rph Where rph.UsuarioRemocao is null and rph.DataHoraRemocao is null and rph.Id = {hotelInput.Id.Value} and rph.RegraPaxFree.Id = {regra.Id}")).FirstOrDefault();
                    }

                    // Verificar se já existe hotel com mesmo HotelId para esta regra
                    if (hotel == null)
                    {
                        hotel = (await _repository.FindByHql<RegraPaxFreeHotel>($"From RegraPaxFreeHotel rph Where rph.UsuarioRemocao is null and rph.DataHoraRemocao is null and rph.HotelId = {hotelInput.HotelId} and rph.RegraPaxFree.Id = {regra.Id}")).FirstOrDefault();
                    }

                    if (hotel == null)
                    {
                        hotel = new RegraPaxFreeHotel();
                        hotel.RegraPaxFree = regra;
                    }

                    hotel.HotelId = hotelInput.HotelId;

                    await _repository.Save(hotel);
                }
            }
        }
    }
}

