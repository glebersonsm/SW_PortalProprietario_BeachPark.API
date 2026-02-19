using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IAuthService
    {
        Task<UserRegisterResultModel> Register(UserRegisterInputModel userInputModel);
        Task<TokenResultModel?> Login(LoginInputModel userLoginInputModel);
        Task<TokenResultModel> ChangeActualCompanyId(SetCompanyModel model);
        Task<Login2FAOptionsResultModel> GetLogin2FAOptionsAsync(string login);
        /// <summary>
        /// Envia o cÃ³digo 2FA pelo canal escolhido (sem pedir senha).
        /// Usado quando o login tem 2FA habilitado.
        /// </summary>
        Task<TokenResultModel?> SendTwoFactorCodeAsync(SendTwoFactorCodeInputModel model);
        Task<TokenResultModel?> ValidateTwoFactorAsync(ValidateTwoFactorInputModel model);
        /// <summary>
        /// Lista comunicaÃ§Ãµes de token 2FA enviadas (para auditoria e gerenciamento de volume).
        /// </summary>
        Task<(int pageNumber, int lastPageNumber, List<ComunicacaoTokenEnviadaViewModel> list)?> SearchComunicacoesTokenEnviadasAsync(SearchComunicacaoTokenEnviadaModel search);
        /// <summary>
        /// Retorna resumo de volume de comunicaÃ§Ãµes de token por canal no perÃ­odo.
        /// </summary>
        Task<List<ResumoVolumeComunicacaoTokenModel>> GetResumoVolumeComunicacoesTokenAsync(DateTime? dataInicial, DateTime? dataFinal);
    }
}
