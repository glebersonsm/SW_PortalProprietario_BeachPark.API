using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IVhfConfigService
    {
        /// <summary>
        /// Retorna todas as opÃ§Ãµes disponÃ­veis para configuraÃ§Ã£o de reservas VHF (PMS).
        /// Inclui: Tipo de utilizaÃ§Ã£o, HotÃ©is, Tipo de HÃ³spede, Origem, Tarifa Hotel, CÃ³digo de PensÃ£o.
        /// </summary>
        Task<VhfConfigOpcoesModel> GetOpcoesAsync();

        Task<List<VhfConfigModel>> GetAllAsync();
        Task<VhfConfigModel?> GetByIdAsync(int id);
        Task<VhfConfigModel> CreateAsync(VhfConfigInputModel model);
        Task<VhfConfigModel> UpdateAsync(VhfConfigInputModel model);
        Task<bool> DeleteAsync(int id);
    }
}
