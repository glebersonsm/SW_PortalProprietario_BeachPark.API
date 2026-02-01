using SW_Utils.Models;

namespace SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround
{
    public interface ILogMessageToQueueProducer
    {
        Task AddLogMessage();
        Task AddLogMessage(OperationSystemLogModelEvent message);
    }
}
