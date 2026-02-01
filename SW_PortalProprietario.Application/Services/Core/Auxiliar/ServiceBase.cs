using Dapper;
using FluentNHibernate.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_Utils.Functions;
using SW_Utils.Models;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core.Auxiliar
{
    public class ServiceBase : IServiceBase
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<IServiceBase> _logger;
        private readonly ICacheStore _cache;
        private readonly ILogMessageToQueueProducer _queueSender;
        private readonly IConfiguration _configuration;
        private readonly List<UsuarioPessoaModel>? _users;
        private readonly ICommunicationProvider _communicationProvider;

        private readonly OperationSystemLogModelEvent operationSystemLogModel = new OperationSystemLogModelEvent();

        public string RequestArguments { get; set; } = "";
        public string ResponseData { get; set; } = "";
        public int? UsuarioId { get; set; }

        public string? GetProviderName => throw new NotImplementedException();

        public ServiceBase(IRepositoryNH repository,
            ILogger<IServiceBase> logger,
            ICacheStore cache, ILogMessageToQueueProducer queueSender,
            IConfiguration configuration, 
            ICommunicationProvider communicationProvider)
        {
            _repository = repository;
            _logger = logger;
            _cache = cache;
            _queueSender = queueSender;
            _configuration = configuration;
            operationSystemLogModel = new OperationSystemLogModelEvent()
            {
                DataInicio = DateTime.Now
            };
            _communicationProvider = communicationProvider;
        }

        public void Compare(EntityBaseCore? objOld, EntityBaseCore? newObject)
        {
            if (objOld == null && newObject == null) return;
            var resultCompare = Helper.CompareObjects(objOld, newObject);

            resultCompare.ObjectGuid = objOld != null ? objOld.ObjectGuid : newObject?.ObjectGuid;
            resultCompare.ObjectId = objOld != null ? objOld.Id : newObject?.Id;
            resultCompare.ObjectType = objOld != null ? objOld.GetType().AssemblyQualifiedName : newObject?.GetType().AssemblyQualifiedName;
            resultCompare.DataHoraOperacao = DateTime.Now;
            var usuarioOperacao = _repository.GetLoggedUser().Result;
            if (!string.IsNullOrEmpty(usuarioOperacao.Value.userId))
                resultCompare.UsuarioOperacao = Convert.ToInt32(usuarioOperacao.Value.userId);
            operationSystemLogModel.Modificacoes.Add(resultCompare);
        }

        public async Task<T?> GetObjectOld<T>(T entity) where T : EntityBaseCore
        {
            var objBd = await _repository.FindById<T>(entity.Id);
            if (objBd != null)
            {
                return objBd;
            }
            else return null;
        }

        public async Task<T?> GetObjectOld<T>(int id) where T : EntityBaseCore
        {
            var objBd = await _repository.FindById<T>(id);
            if (objBd != null)
            {
                return objBd;
            }
            else return null;
        }

        public async Task AddLogAuditoriaMessageToQueue(HttpContext httpContext, int status)
        {
            try
            {
                var strController = httpContext.Request.Path.Value;
                var strControllerNormalized = strController?.ToString();
                if (strControllerNormalized != null && strControllerNormalized.Contains("."))
                {
                    var arr = strControllerNormalized.Split('.').TakeLast(2).AsList();
                    strControllerNormalized = string.Join(".", arr);
                }

                if (UsuarioId.GetValueOrDefault(0) == 0)
                {
                    var dadosContext = httpContext?.User;
                    if (dadosContext != null && dadosContext.Claims.Any(b => b.Type != null && b.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var idUser = dadosContext.Claims.FirstOrDefault(b => b.Type != null && b.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase));
                        if (idUser != null && !string.IsNullOrEmpty(idUser.Value))
                            UsuarioId = Convert.ToInt32(idUser.Value);

                    }
                }

                var ipAddress = httpContext?.Connection?.RemoteIpAddress;
                operationSystemLogModel.DataFinal = DateTime.Now;
                operationSystemLogModel.UrlRequested = $"{httpContext?.Request.Scheme} - {httpContext?.Request.Host} - {strControllerNormalized ?? strController?.ToString()} - {httpContext?.Request.Path}";
                operationSystemLogModel.RequestBody = RequestArguments;
                operationSystemLogModel.Response = ResponseData;
                operationSystemLogModel.ClientIpAddress = $"{ipAddress}";
                operationSystemLogModel.StatusResult = status;
                operationSystemLogModel.UsuarioId = UsuarioId.GetValueOrDefault(-1);
                try
                {
                    string userId = "";
                    var userLogged = _repository.GetLoggedUser().Result;
                    if (userLogged == null || userLogged.Value.userId == null)
                        userId = $"{UsuarioId.GetValueOrDefault(-1)}";

                    if (operationSystemLogModel.UsuarioId.GetValueOrDefault(0) <= 0)
                        operationSystemLogModel.UsuarioId = Convert.ToInt32(userId);
                }
                catch (Exception)
                {
                }

                var gravarLogEmFila = _configuration.GetValue<bool>("SendOperationsToProcessingLogQueue");

                var gravado = gravarLogEmFila ? await GravarLogFilaProcessamento(operationSystemLogModel) : false;
                if (!gravado)
                {
                    gravado = await GravarLogCacheDistribuido(operationSystemLogModel);
                }

                _logger.LogInformation($"{DateTime.Now} - Evento de log de operações adicionado na fila para processamento");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"{DateTime.Now} - Não foi possível gravar o log de auditoria da requisição");
                throw;
            }
        }

        private async Task<bool> GravarLogFilaProcessamento(OperationSystemLogModelEvent operationSystemLogModel)
        {
            try
            {
                await _queueSender.AddLogMessage(operationSystemLogModel);
                return true;
            }
            catch (Exception err)
            {
                _logger.LogWarning($"{DateTime.Now} - Não foi possível gravar o log na fila de processamento! Message: {err.Message} Inner: {err.InnerException?.Message} StackTrace: {err.StackTrace}");
                return false;
            }
        }

        private async Task<bool> GravarLogCacheDistribuido(OperationSystemLogModelEvent operationSystemLogModel)
        {
            try
            {
                await _cache.AddAsync($"operationSystemExecuted:{operationSystemLogModel.UsuarioId}:{operationSystemLogModel.UrlRequested.RemoveAccents().Replace(' ', '-')}_{DateTime.Now:yyyyMMddHHmmss}_{operationSystemLogModel.Guid}", operationSystemLogModel, DateTimeOffset.Now.AddDays(100), 0, _repository.CancellationToken);
                return true;
            }
            catch (Exception err)
            {
                _logger.LogWarning($"{DateTime.Now} - Não foi possível gravar o log na cache distribuido! Message: {err.Message} Inner: {err.InnerException?.Message} StackTrace: {err.StackTrace}");
                return false;
            }
        }

        public async Task<T> SetUserName<T>(T model) where T : ModelBase
        {
            if (_users != null && _users.Any())
            {
                if (model.UsuarioCriacao.GetValueOrDefault(0) > 0)
                {
                    var usuario = _users.FirstOrDefault(a => a.UsuarioId == model.UsuarioCriacao);
                    if (usuario != null)
                    {
                        model.NomeUsuarioCriacao = usuario.NomePessoa;
                    }
                }

                if (model.UsuarioAlteracao.GetValueOrDefault(0) > 0)
                {
                    var usuario = _users.FirstOrDefault(a => a.UsuarioId == model.UsuarioAlteracao);
                    if (usuario != null)
                    {
                        model.NomeUsuarioAlteracao = usuario.NomePessoa;
                    }
                }
            }

            return await Task.FromResult(model);

        }

        public async Task<List<T>> SetUserName<T>(List<T> models) where T : ModelBase
        {
            if (_users != null && _users.Any())
            {
                foreach (var modelGroup in models.Where(c => c.UsuarioCriacao.GetValueOrDefault(0) > 0)
                    .GroupBy(c => c.UsuarioCriacao.GetValueOrDefault(0)))
                {

                    var usuario = _users.FirstOrDefault(a => a.UsuarioId == modelGroup.Key);
                    if (usuario != null)
                    {
                        foreach (var itemModel in modelGroup)
                        {
                            itemModel.NomeUsuarioCriacao = usuario.NomePessoa;
                        }
                    }

                }

                foreach (var modelGroup in models.Where(c => c.UsuarioAlteracao.GetValueOrDefault(0) > 0)
                    .GroupBy(c => c.UsuarioAlteracao.GetValueOrDefault(0)))
                {

                    var usuario = _users.FirstOrDefault(a => a.UsuarioId == modelGroup.Key);
                    if (usuario != null)
                    {
                        foreach (var itemModel in modelGroup)
                        {
                            itemModel.NomeUsuarioAlteracao = usuario.NomePessoa;
                        }
                    }

                }
            }

            return await Task.FromResult(models.AsList());

        }

        private async Task<List<PessoaSistemaXProviderModel>> GetUserXProvider(string provider, string pessoaProvider = "", string pessoaSistema = "")
        {

            var dbType = _repository.DataBaseType;

            var providerBasePesquisa = $"'{provider}'";
            if (!provider.Contains("provider", StringComparison.CurrentCultureIgnoreCase))
            {
                providerBasePesquisa += $",'{provider}provider'";
            }
            else
            {
                providerBasePesquisa += $",'{provider.Replace("provider", "", StringComparison.CurrentCultureIgnoreCase)}'";
            }

            var sb = new StringBuilder(@$"Select 
                                        a.NomeProvider,
                                        a.PessoaSistema,
                                        a.PessoaProvider,
                                        (Select Max(u.Id) From Usuario u Where u.Pessoa = a.PessoaSistema and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0) as UsuarioSistemaId
                                        From 
                                        PessoaSistemaXProvider a
                                        Where 1 = 1 ");

            if (dbType == SW_Utils.Enum.EnumDataBaseType.PostgreSql)
            {
                sb = new StringBuilder(@$"Select 
                                        a.NomeProvider,
                                        a.PessoaSistema,
                                        a.PessoaProvider,
                                        (Select Max(u.Id) From Usuario u Where u.Pessoa = Cast(a.PessoaSistema as int) and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0) as UsuarioSistemaId
                                        From 
                                        PessoaSistemaXProvider a
                                        Where 1 = 1 ");
            }

            if (!string.IsNullOrEmpty(provider))
                sb.AppendLine($" and Lower(a.NomeProvider) in ({providerBasePesquisa.ToLower()})");

            if (!string.IsNullOrEmpty(pessoaProvider))
                sb.AppendLine($" and a.PessoaProvider = '{pessoaProvider}' ");

            if (!string.IsNullOrEmpty(pessoaSistema))
                sb.AppendLine($" and a.PessoaSistema = '{pessoaSistema}' ");


            return (await _repository.FindBySql<PessoaSistemaXProviderModel>(sb.ToString())).AsList();

        }

        public async Task<PessoaSistemaXProviderModel?> GetPessoaSistemaVinculadaPessoaProvider(string pessoaProvider, string providerName)
        {
            var result = await GetUserXProvider(providerName, pessoaProvider);
            if (result != null && result.Any())
                return result.FirstOrDefault();
            return null;
        }

        public async Task<PessoaSistemaXProviderModel?> GetPessoaProviderVinculadaPessoaSistema(string pessoaSistema, string providerName)
        {
            var result = await GetUserXProvider(providerName, "", pessoaSistema);
            if (result != null && result.Any())
                return result.FirstOrDefault();
            return null;
        }

        public async Task<PessoaSistemaXProviderModel?> GetPessoaProviderVinculadaUsuarioSistema(int usuarioSistemaId, string? providerName = null)
        {
            var dbType = _repository.DataBaseType;

            if (providerName == null)
                providerName = _communicationProvider.CommunicationProviderName;

            var usuario = (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Id = {usuarioSistemaId} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
            if (usuario == null)
                throw new ArgumentException($"Não foi encontrado o usuário com Id: {usuarioSistemaId}");

            var sb = new StringBuilder(@$"Select 
                                        a.NomeProvider,
                                        a.PessoaSistema,
                                        a.PessoaProvider,
                                        (Select Max(u.Id) From Usuario u Where u.Pessoa = a.PessoaSistema and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0) as UsuarioSistemaId
                                        From 
                                        PessoaSistemaXProvider a
                                        Where 1 = 1 and 
                                              a.PessoaSistema = {usuario.Pessoa?.Id}");

            if (dbType == SW_Utils.Enum.EnumDataBaseType.PostgreSql)
            {
                sb = new StringBuilder(@$"Select 
                                        a.NomeProvider,
                                        a.PessoaSistema,
                                        a.PessoaProvider,
                                        (Select Max(u.Id) From Usuario u Where cast(u.Pessoa as int) = cast(a.PessoaSistema as int) and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0) as UsuarioSistemaId
                                        From 
                                        PessoaSistemaXProvider a
                                        Where 1 = 1 and 
                                              a.PessoaSistema =  '{usuario.Pessoa?.Id}' ");
            }


            if (!string.IsNullOrEmpty(providerName))
                sb.AppendLine($" and Lower(a.NomeProvider) = '{providerName.ToLower()}' ");

            return (await _repository.FindBySql<PessoaSistemaXProviderModel>(sb.ToString())).FirstOrDefault();

        }

        public async Task<ParametroSistemaViewModel?> GetParametroSistema(string? pessoaProviderId = null)
        {
            ParametroSistemaViewModel? parametroSistema = null;
            var loggedUser = await _repository.GetLoggedUser();
            if (string.IsNullOrEmpty(loggedUser.Value.userId)) return default;
            var userId = 0;
            bool? isAdm = false;
            if (pessoaProviderId is null)
            {
                if (loggedUser != null && loggedUser.Value.userId != null)
                {
                    var pessoaProviderDados = await GetPessoaProviderVinculadaUsuarioSistema(int.Parse(loggedUser.Value.userId));
                    if (pessoaProviderDados != null) 
                    {
                        pessoaProviderId = pessoaProviderDados.PessoaProvider;
                    }

                    isAdm = loggedUser?.isAdm;
                }

            }

            userId = loggedUser != null && !string.IsNullOrEmpty(loggedUser.Value.userId) ? int.Parse(loggedUser?.userId!) : 0;

            if (pessoaProviderId != null)
            {
                var outrosDadosPessoa = await _communicationProvider.GetOutrosDadosPessoaProvider(pessoaProviderId!);
                if (outrosDadosPessoa != null && outrosDadosPessoa.EmpreendimentoId.GetValueOrDefault(0) > 0)
                {
                    parametroSistema = await _repository.GetParametroSistemaViewModel();
                    if (parametroSistema != null)
                        parametroSistema.EmpreendimentoId = outrosDadosPessoa.EmpreendimentoId;
                }
                else parametroSistema = await _repository.GetParametroSistemaViewModel();
            }
            else
            {
                parametroSistema = await _repository.GetParametroSistemaViewModel();
            }

            var usuario = (await _repository.FindByHql<Usuario>($"From Usuario u Where u.Id = {userId} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();

            if (isAdm.GetValueOrDefault(false) == false || (usuario != null && (usuario.GestorFinanceiro.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Não && 
                usuario.GestorReservasAgendamentos.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Não)))
            {
                var imagensConfig = _configuration.GetValue<string>("ColecaoImagemPorEmpreendimento");
                if (imagensConfig != null && parametroSistema != null && parametroSistema.EmpreendimentoId.GetValueOrDefault(0) > 0)
                {
                    parametroSistema = await SepararImagens(parametroSistema, imagensConfig, isAdm);
                }
            }

            return parametroSistema;
        }

        private async Task<ParametroSistemaViewModel?> SepararImagens(ParametroSistemaViewModel parametroSistema, string imagensConfig, bool? isAdm = false)
        {
            var idsImagens = imagensConfig.Split('|').FirstOrDefault(a => a.Split('=')[0].ToLowerInvariant() == parametroSistema.EmpreendimentoId.ToLowerInvariantString());
            if (!string.IsNullOrEmpty(idsImagens) && idsImagens.Contains("=") && idsImagens.Contains("[") && idsImagens.Contains("]"))
            {
                var parametro = (await _repository.FindByHql<ParametroSistema>($"From ParametroSistema p Where p.Empresa.Id = {parametroSistema.EmpresaId} and p.Id = {parametroSistema.Id}")).FirstOrDefault();
                if (parametro != null)
                {
                    List<string> imagensExibir = new List<string>();
                    var idsImagensConsiderar = (idsImagens.Split('=')[1]).Replace("[", "").Replace("]", "").Split(',').AsList();
                    if (isAdm.GetValueOrDefault(false) == true)
                    {
                        idsImagensConsiderar = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" };
                    }


                    foreach (var item in idsImagensConsiderar.Where(a => Helper.IsNumeric(a)).Select(c => Convert.ToInt32(c)).ToList())
                    {
                        if (item == 1)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl1))
                                imagensExibir.Add(parametro.ImagemHomeUrl1);
                        }
                        else if (item == 2)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl2))
                                imagensExibir.Add(parametro.ImagemHomeUrl2);
                        }
                        else if (item == 3)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl3))
                                imagensExibir.Add(parametro.ImagemHomeUrl3);
                        }
                        else if (item == 4)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl4))
                                imagensExibir.Add(parametro.ImagemHomeUrl4);
                        }
                        else if (item == 5)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl5))
                                imagensExibir.Add(parametro.ImagemHomeUrl5);
                        }
                        else if (item == 6)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl6))
                                imagensExibir.Add(parametro.ImagemHomeUrl6);
                        }
                        else if (item == 7)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl7))
                                imagensExibir.Add(parametro.ImagemHomeUrl7);
                        }
                        else if (item == 8)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl8))
                                imagensExibir.Add(parametro.ImagemHomeUrl8);
                        }
                        else if (item == 9)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl9))
                                imagensExibir.Add(parametro.ImagemHomeUrl9);
                        }
                        else if (item == 10)
                        {
                            if (!string.IsNullOrEmpty(parametroSistema.ImagemHomeUrl10))
                                imagensExibir.Add(parametroSistema.ImagemHomeUrl10);
                        }
                        else if (item == 11)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl11))
                                imagensExibir.Add(parametro.ImagemHomeUrl11);
                        }
                        else if (item == 12)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl12))
                                imagensExibir.Add(parametro.ImagemHomeUrl12);
                        }
                        else if (item == 13)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl13))
                                imagensExibir.Add(parametro.ImagemHomeUrl13);
                        }
                        else if (item == 14)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl14))
                                imagensExibir.Add(parametro.ImagemHomeUrl14);
                        }
                        else if (item == 15)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl15))
                                imagensExibir.Add(parametro.ImagemHomeUrl15);
                        }
                        else if (item == 16)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl16))
                                imagensExibir.Add(parametro.ImagemHomeUrl16);
                        }
                        else if (item == 17)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl17))
                                imagensExibir.Add(parametro.ImagemHomeUrl17);
                        }
                        else if (item == 18)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl18))
                                imagensExibir.Add(parametro.ImagemHomeUrl18);
                        }
                        else if (item == 19)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl19))
                                imagensExibir.Add(parametro.ImagemHomeUrl19);
                        }
                        else if (item == 20)
                        {
                            if (!string.IsNullOrEmpty(parametro.ImagemHomeUrl20))
                                imagensExibir.Add(parametro.ImagemHomeUrl20);
                        }
                    }

                    int index = 0;
                    foreach (var item in imagensExibir)
                    {
                        if (index == 0)
                            parametroSistema.ImagemHomeUrl1 = item;
                        else if (index == 1)
                            parametroSistema.ImagemHomeUrl2 = item;
                        else if (index == 2)
                            parametroSistema.ImagemHomeUrl3 = item;
                        else if (index == 3)
                            parametroSistema.ImagemHomeUrl4 = item;
                        else if (index == 4)
                            parametroSistema.ImagemHomeUrl5 = item;
                        else if (index == 5)
                            parametroSistema.ImagemHomeUrl6 = item;
                        else if (index == 6)
                            parametroSistema.ImagemHomeUrl7 = item;
                        else if (index == 7)
                            parametroSistema.ImagemHomeUrl8 = item;
                        else if (index == 8)
                            parametroSistema.ImagemHomeUrl9 = item;
                        else if (index == 9)
                            parametroSistema.ImagemHomeUrl10 = item;

                        index++;
                    }

                    for (int i = index; i < 20; i++)
                    {
                        if (i == 0)
                            parametroSistema.ImagemHomeUrl1 = null;
                        else if (i == 1)
                            parametroSistema.ImagemHomeUrl2 = null;
                        else if (i == 2)
                            parametroSistema.ImagemHomeUrl3 = null;
                        else if (i == 3)
                            parametroSistema.ImagemHomeUrl4 = null;
                        else if (i == 4)
                            parametroSistema.ImagemHomeUrl5 = null;
                        else if (i == 5)
                            parametroSistema.ImagemHomeUrl6 = null;
                        else if (i == 6)
                            parametroSistema.ImagemHomeUrl7 = null;
                        else if (i == 7)
                            parametroSistema.ImagemHomeUrl8 = null;
                        else if (i == 8)
                            parametroSistema.ImagemHomeUrl9 = null;
                        else if (i == 9)
                            parametroSistema.ImagemHomeUrl10 = null;
                        else if (i == 10)
                            parametroSistema.ImagemHomeUrl11 = null;
                        else if (i == 11)
                            parametroSistema.ImagemHomeUrl12 = null;
                        else if (i == 12)
                            parametroSistema.ImagemHomeUrl13 = null;
                        else if (i == 13)
                            parametroSistema.ImagemHomeUrl14 = null;
                        else if (i == 14)
                            parametroSistema.ImagemHomeUrl15 = null;
                        else if (i == 16)
                            parametroSistema.ImagemHomeUrl17 = null;
                        else if (i == 17)
                            parametroSistema.ImagemHomeUrl18 = null;
                        else if (i == 18)
                            parametroSistema.ImagemHomeUrl19 = null;
                        else if (i == 19)
                            parametroSistema.ImagemHomeUrl20 = null;
                    }
                }

            }
            return parametroSistema;
        }

        public async Task<string> getToken()
        {
            return await _repository.GetToken();
        }

        public async Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas()
        {
            var loggedUser = await _repository.GetLoggedUser();
            if (loggedUser == null)
                throw new ArgumentException("Não foi possível obter o usuário logado.");

            var parametro = (await _repository.FindBySql<ParametroSistemaViewModel>($"Select p.ExibirFinanceirosDasEmpresaIds From ParametroSistema p Where 1 = 1 Order by p.Id Desc")).FirstOrDefault();
            if (parametro == null)
                throw new ArgumentException("Não foi possível obter o parâmetro do sistema.");

            var empresasIds = !string.IsNullOrEmpty(parametro.ExibirFinanceirosDasEmpresaIds) ? parametro.ExibirFinanceirosDasEmpresaIds.Split(',').AsList() : new();

            if (empresasIds == null || empresasIds.Count == 0)
                throw new ArgumentException("Não foi possível obter as empresas vinculadas.");

            var empresasVinculadas = (await _communicationProvider.GetEmpresasVinculadas(empresasIds)).AsList();

            return empresasVinculadas;
        }

        public async Task<List<DadosContratoModel>?> GetContratos(List<int> pessoasPesquisar)
        {
            return await _communicationProvider.GetContratos(pessoasPesquisar);
        }
    }
}
