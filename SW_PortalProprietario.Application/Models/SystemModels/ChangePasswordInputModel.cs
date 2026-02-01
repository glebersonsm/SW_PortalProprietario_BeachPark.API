using SW_PortalProprietario.Application.Auxiliar;
using System.Text.Json.Serialization;

namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class ChangePasswordInputModel
    {
        [JsonConverter(typeof(EncryptConverter))]

        public string? ActualPassword { get; set; }
        [JsonConverter(typeof(EncryptConverter))]

        public string? NewPassword { get; set; }

        [JsonConverter(typeof(EncryptConverter))]

        public string? NewPasswordConfirmation { get; set; }

    }
}
