
using SW_PortalProprietario.Application.Services.Providers.Esolution;

namespace SW_PortalProprietario.Application.Models.AuthModels
{
    public class LoginResult
    {
        public int? code { get; set; }
        public string? token { get; set; }
        public string? message { get; set; }
        public DadosClienteLegado? dadosCliente { get; set; }
    }
}
