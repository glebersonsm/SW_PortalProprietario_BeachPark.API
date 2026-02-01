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

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
