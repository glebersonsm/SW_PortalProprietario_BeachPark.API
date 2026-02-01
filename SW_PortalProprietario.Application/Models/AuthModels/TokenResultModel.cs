using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Application.Models.AuthModels
{
    public class TokenResultModel
    {
        public int? UserId { get; set; }
        public string? ProviderKeyUser { get; set; }
        public string? CompanyId { get; set; }
        public string? Login { get; set; }
        public string? Token { get; set; }
        public int? Idioma { get; set; } //0 = Português, 1 = Inglês, 2 = Espanhol
        public int? PodeInformarConta { get; set; } //0 = Não, 1 = Sim
        public int? PodeInformarPix { get; set; } //0 = Não, 1 = Sim
        public DateTime? FimValidade { get; set; }
        public bool IsAdmin { get; internal set; }
        public bool IsGestorReservasAgendamentos { get; internal set; }
        public bool IsGestorFinanceiro { get; internal set; }
        public string? IdIntercambiadora { get; set; }
        public string? PessoaTitular1Tipo { get; set; }
        public string? PessoaTitular1CPF { get; set; }
        public string? PessoaTitular1CNPJ { get; set; }
        public string? EmpreendimnentoNome { get; set; }
        public string? PadraoDeCor { get; set; } = "Default";
        public List<ContratoResultModel>? Contratos { get; set; } = new List<ContratoResultModel>();

        public static explicit operator TokenResultModel(Usuario usuario)
        {
            return new TokenResultModel
            {
                UserId = usuario.Id,
                Login = usuario.Login,
                ProviderKeyUser = usuario.ProviderChaveUsuario
            };
        }

    }
}
