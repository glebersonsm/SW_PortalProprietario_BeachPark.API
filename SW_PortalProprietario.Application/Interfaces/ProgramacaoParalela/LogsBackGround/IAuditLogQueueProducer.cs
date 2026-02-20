using SW_Utils.Models;

namespace SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround
{
    public interface IAuditLogQueueProducer
    {
        Task EnqueueAuditLogAsync(AuditLogMessageEvent message);
    }
}

