using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Services.Core;
using SW_PortalProprietario.Application.Services.Core.Interfaces.ProgramacaoParalela;
using SW_PortalProprietario.Domain.Entities.Core.Framework.GerenciamentoAcesso;
using SW_Utils.Models;

namespace SW_PortalProprietario.Application.Services.ProgramacaoParalela.LogsBackGround
{
    public class LogOperationService : ILogOperationService
    {
        private readonly IRepositoryHosted _repository;
        private readonly ILogger<StateService> _logger;
        public LogOperationService(IRepositoryHosted repository,
            ILogger<StateService> logger)
        {
            _repository = repository;
            _logger = logger;
        }


        public async Task<bool> SaveLog(OperationSystemLogModelEvent log)
        {
            using (var session = _repository.CreateSession())
            {
                var intLogId = 0;
                try
                {
                    _repository.BeginTransaction(session);
                    var logAcesso = new LogAcesso();
                    logAcesso.Guid = $"{log.Guid}";
                    logAcesso.DataInicio = log.DataInicio;
                    logAcesso.DataFinal = log.DataFinal;
                    logAcesso.UrlRequested = log.UrlRequested;
                    logAcesso.ClientIpAddress = log.ClientIpAddress;
                    logAcesso.RequestBody = log.RequestBody;
                    logAcesso.Response = log.Response;
                    logAcesso.StatusResult = log.StatusResult;
                    logAcesso.UsuarioCriacao = log.UsuarioId;

                    var logAcessoResult = await _repository.ForcedSave(logAcesso, session);
                    intLogId = logAcessoResult.Id;
                    if (log.Modificacoes != null && log.Modificacoes.Any())
                    {
                        foreach (var itemModificado in log.Modificacoes)
                        {
                            var logAcessoObjeto = new LogAcessoObjeto();
                            logAcessoObjeto.ObjectType = itemModificado.ObjectType;
                            logAcessoObjeto.ObjectOperationGuid = itemModificado.ObjectGuid;
                            logAcessoObjeto.ObjectId = itemModificado.ObjectId;
                            logAcessoObjeto.DataHoraOperacao = itemModificado.DataHoraOperacao;
                            logAcessoObjeto.UsuarioOperacao = itemModificado.UsuarioOperacao;
                            logAcessoObjeto.TipoOperacao = itemModificado.TipoOperacao;
                            logAcessoObjeto.UsuarioCriacao = itemModificado.UsuarioOperacao;
                            var logAcessoObjetoResult = await _repository.ForcedSave(logAcessoObjeto, session);

                            foreach (var itemCampo in itemModificado.Modificacoes)
                            {
                                var logAcessoObjetoCampo = new LogAcessoObjetoCampo();
                                logAcessoObjetoCampo.TipoCampo = itemCampo.TipoCampo;
                                logAcessoObjetoCampo.NomeCampo = itemCampo.NomeCampo;
                                logAcessoObjetoCampo.ValorAntes = $"{itemCampo.ValorAntes ?? ""}";
                                logAcessoObjetoCampo.ValorApos = $"{itemCampo.ValorApos ?? ""}";
                                logAcessoObjetoCampo.UsuarioCriacao = itemModificado.UsuarioOperacao;
                                var logAcessoObjetoCampoResult = await _repository.ForcedSave(logAcessoObjetoCampo, session);
                            }
                        }
                    }

                    var resultCommit = await _repository.CommitAsync(session);

                    return await Task.FromResult(true);
                }
                catch (Exception err)
                {
                    _logger.LogError(err, $"Não foi possível salvar o log de operações: ({intLogId})");
                    _repository.Rollback(session);
                    return await Task.FromResult(false);
                }
            }
        }

    }
}
