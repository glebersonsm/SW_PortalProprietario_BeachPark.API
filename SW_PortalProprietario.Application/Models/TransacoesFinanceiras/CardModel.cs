using SW_PortalProprietario.Application.Auxiliar;
using System.Text.Json.Serialization;

namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class CardModel
    {
        public string? brand { get; set; }
        [JsonConverter(typeof(EncryptConverter))]
        public string? card_number { get; set; }
        [JsonConverter(typeof(EncryptConverter))]
        public string? cvv { get; set; }
        public string? due_date { get; set; }
        public string? card_holder { get; set; }
    }
}
