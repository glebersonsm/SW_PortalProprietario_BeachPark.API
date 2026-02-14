using CMDomain.Entities;
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
using System.Configuration.Provider;
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

        public string? GetProviderName => _communicationProvider.CommunicationProviderName;

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

        public async Task<List<PessoaSistemaXProviderModel>?> GetPessoaSistemaVinculadaPessoaProvider(string pessoaProvider, string? providerName = "esolution")
        {
            var result = await GetUserXProvider(provider: providerName ?? "esolution", pessoaProvider: pessoaProvider);
            return result;
        }

        public async Task<List<PessoaSistemaXProviderModel>?> GetPessoaProviderVinculadaUsuarioSistema(int usuarioSistemaId, string? providerName = "esolution")
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
                sb.AppendLine($" and Lower(a.NomeProvider) in ('esolution','cm','{providerName.ToLower()}') ");

            return (await _repository.FindBySql<PessoaSistemaXProviderModel>(sb.ToString())).AsList();

        }

        public async Task<ParametroSistemaViewModel?> GetParametroSistema(string? pessoaProviderId = null, string? providerName = "esolution")
        {
            ParametroSistemaViewModel? parametroSistema = null;
            List<PessoaSistemaXProviderModel>? pessoaProviderDados = null;

            var loggedUser = await _repository.GetLoggedUser();
            if (string.IsNullOrEmpty(loggedUser.Value.userId)) return default;
            var userId = 0;
            bool? isAdm = false;
            if (pessoaProviderId is null)
            {
                if (loggedUser != null && loggedUser.Value.userId != null)
                {
                    pessoaProviderDados = await GetPessoaProviderVinculadaUsuarioSistema(int.Parse(loggedUser.Value.userId), providerName) ?? new List<PessoaSistemaXProviderModel>();
                    isAdm = loggedUser?.isAdm;
                }
            }
            else
            {
                pessoaProviderDados = await GetPessoaSistemaVinculadaPessoaProvider(pessoaProviderId, _communicationProvider.CommunicationProviderName);
            }

             userId = loggedUser != null && !string.IsNullOrEmpty(loggedUser.Value.userId) ? int.Parse(loggedUser?.userId!) : 0;

            if (pessoaProviderId != null)
            {
                if (pessoaProviderDados != null && pessoaProviderDados.Any())
                {
                    foreach (var item in pessoaProviderDados.Where(b => !string.IsNullOrEmpty(b.NomeProvider) && b.NomeProvider.Contains("esol", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        item.VinculoAccess = await _communicationProvider.GetOutrosDadosPessoaProvider(item.PessoaProvider!);
                    }
                        
                    //    if (outrosDadosPessoa != null && outrosDadosPessoa.EmpreendimentoId.GetValueOrDefault(0) > 0)
                    //    {
                    //        parametroSistema = await _repository.GetParametroSistemaViewModel();
                    //        if (parametroSistema != null)
                    //            parametroSistema.EmpreendimentoId = outrosDadosPessoa.EmpreendimentoId;
                    //    }
                    //    else parametroSistema = await _repository.GetParametroSistemaViewModel();
                    //}
                    
                }
            
            }
            else
            {
                parametroSistema = await _repository.GetParametroSistemaViewModel();
            }

            var usuario = (await _repository.FindByHql<Usuario>($"From Usuario u Where u.Id = {userId} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();

            return parametroSistema;
        }


        public async Task<string> getToken()
        {
            return await _repository.GetToken();
        }

        public async Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas(string providerName = "esolution")
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

            var empresasVinculadas = (await _communicationProvider.GetEmpresasVinculadas(empresasIds, providerName)).AsList();

            return empresasVinculadas;
        }

        public async Task<List<DadosContratoModel>?> GetContratos(List<int> pessoasPesquisar, string providerName = "esolution")
        {
            return await _communicationProvider.GetContratos(pessoasPesquisar, providerName);
        }

        public async Task<List<PessoaSistemaXProviderModel>?> GetPessoaProviderVinculadaPessoaSistema(string pessoaProvider, string? providerName = "esolution")
        {
            var dbType = _repository.DataBaseType;

            if (providerName == null)
                providerName = _communicationProvider.CommunicationProviderName;

            var sb = new StringBuilder(@$"Select 
                                        a.NomeProvider,
                                        a.PessoaSistema,
                                        a.PessoaProvider,
                                        (Select Max(u.Id) From Usuario u Where u.Pessoa = a.PessoaSistema and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0) as UsuarioSistemaId
                                        From 
                                        PessoaSistemaXProvider a
                                        Where 1 = 1 and 
                                        a.PessoaProvider = {pessoaProvider}");

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
                                        a.PessoaProvider =  '{pessoaProvider}' ");
            }


            if (!string.IsNullOrEmpty(providerName))
                sb.AppendLine($" and Lower(a.NomeProvider) in ('ESOLUTION','CM','{providerName.ToLower()}') ");

            return (await _repository.FindBySql<PessoaSistemaXProviderModel>(sb.ToString())).AsList();
        }
    }
}
