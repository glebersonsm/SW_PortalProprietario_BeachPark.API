using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IParametroSistemaService
    {
        Task<ParametroSistemaViewModel?> SaveParameters(ParametroSistemaInputUpdateModel model);
        Task<ParametroSistemaViewModel?> GetParameters();
        /// <summary>
        /// Atualiza apenas o parÃ¢metro TipoEnvioEmail (ex.: apÃ³s fallback de envio ter sucesso pelo mÃ©todo alternativo).
        /// NÃ£o utiliza HttpContext; seguro para chamada a partir de hosted services.
        /// </summary>
        Task UpdateTipoEnvioEmailOnlyAsync(EnumTipoEnvioEmail tipoEnvioEmail, CancellationToken cancellationToken = default);
    }
}
