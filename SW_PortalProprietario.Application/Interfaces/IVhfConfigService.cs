using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IVhfConfigService
    {
        /// <summary>
        /// Retorna todas as opções disponíveis para configuração de reservas VHF (PMS).
        /// Inclui: Tipo de utilização, Hotéis, Tipo de Hóspede, Origem, Tarifa Hotel, Código de Pensão.
        /// </summary>
        Task<VhfConfigOpcoesModel> GetOpcoesAsync();

        Task<List<VhfConfigModel>> GetAllAsync();
        Task<VhfConfigModel?> GetByIdAsync(int id);
        Task<VhfConfigModel> CreateAsync(VhfConfigInputModel model);
        Task<VhfConfigModel> UpdateAsync(VhfConfigInputModel model);
        Task<bool> DeleteAsync(int id);
    }
}
