using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.AuthModels;

namespace SW_PortalProprietario.Application.Services.Providers
{
    public class AccessValidateResultModel : IAccessValidateResultModel
    {
        public string? PessoaId { get; set; }
        public int? UsuarioSistema { get; set; }
        public string? ClienteId { get; set; }
        public string? PessoaNome { get; set; }
        public List<string> InfomacoesAdicionais { get; set; } = new List<string>();
        public List<string> Erros { get; set; } = new List<string>();
        public string? ProviderName { get; set; }
        public LoginResult? LoginResult { get; set; }
    }
}
