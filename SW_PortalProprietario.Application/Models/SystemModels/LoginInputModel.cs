using SW_PortalProprietario.Application.Auxiliar;
using SW_PortalProprietario.Domain.Interfaces;
using System.Text.Json.Serialization;

namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class LoginInputModel : IObjectAuditLog
    {
        public string? Login { get; set; }
        [JsonConverter(typeof(EncryptConverter))]
        public string? Senha { get; set; }
        /// <summary> Canal escolhido para envio do cÃ³digo 2FA: "email" | "sms". ObrigatÃ³rio quando 2FA estÃ¡ habilitado para o perfil. </summary>
        public string? TwoFactorChannel { get; set; }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
