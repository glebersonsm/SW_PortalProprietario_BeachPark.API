using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IParametroSistemaService
    {
        Task<ParametroSistemaViewModel?> SaveParameters(ParametroSistemaInputUpdateModel model);
        Task<ParametroSistemaViewModel?> GetParameters();
    }
}
