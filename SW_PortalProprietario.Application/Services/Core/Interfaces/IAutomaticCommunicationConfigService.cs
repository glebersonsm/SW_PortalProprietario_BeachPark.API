using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IAutomaticCommunicationConfigService
    {
        Task<AutomaticCommunicationConfigModel?> GetByCommunicationTypeAsync(EnumDocumentTemplateType? communicationType = EnumDocumentTemplateType.VoucherReserva, EnumProjetoType? projetoType = null);
        Task<List<AutomaticCommunicationConfigModel>> GetAllAsync(EnumProjetoType? projetoType = null);
        Task<AutomaticCommunicationConfigModel> SaveAsync(AutomaticCommunicationConfigInputModel model);
        Task<AutomaticCommunicationConfigModel> UpdateAsync(int id, AutomaticCommunicationConfigInputModel model);
        Task<bool> DeleteAsync(int id);
        Task<bool> SimulateEmailAsync(int configId, string userEmail);
    }
}

