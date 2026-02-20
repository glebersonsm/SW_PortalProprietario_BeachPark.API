using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IRegraIntercambioService
    {
        Task<RegraIntercambioOpcoesModel> GetOpcoesAsync();
        Task<List<RegraIntercambioModel>> GetAllAsync();
        Task<RegraIntercambioModel?> GetByIdAsync(int id);
        Task<RegraIntercambioModel> CreateAsync(RegraIntercambioInputModel model);
        Task<RegraIntercambioModel> UpdateAsync(RegraIntercambioInputModel model);
        Task<bool> DeleteAsync(int id);
    }
}
