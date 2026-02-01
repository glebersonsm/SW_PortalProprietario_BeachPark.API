using SW_Utils.Models;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces.ProgramacaoParalela
{
    public interface ILogOperationService
    {
        Task<bool> SaveLog(OperationSystemLogModelEvent log);
    }
}
