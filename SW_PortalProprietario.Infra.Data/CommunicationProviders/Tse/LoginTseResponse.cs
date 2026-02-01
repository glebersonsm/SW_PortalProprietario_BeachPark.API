using System.Text.Json.Serialization;

namespace SW_PortalProprietario.Infra.Data.CommunicationProviders.Tse
{
    public class LoginTseResponse
    {
        public string? Exp { get; set; }
        public string? ExpireValue { get; set; }
        public string? UserName { get; set; }

        public string? Partner { get; set; }
        public string? Type { get; set; }
        public TSE_Token? Token { get; set; }
        public DateTime? DataLiberacao { get; set; }
        public DateTime? ValidoAte
        {
            get
            {

                if (!string.IsNullOrEmpty(ExpireValue))
                {
                    if (DataLiberacao == null)
                        DataLiberacao = DateTime.Now.AddSeconds(-50);

                    return DataLiberacao.Value.AddSeconds(Convert.ToInt64(ExpireValue));
                }
                return null;
            }
        }

    }


    public class TSE_Token
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
        [JsonPropertyName("userName")]
        public string? UserName { get; set; }
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }
        [JsonPropertyName(".issued")]
        public string? Issued { get; set; }
        [JsonPropertyName(".expires")]
        public string? Expires { get; set; }
        [JsonPropertyName("expires_in")]
        public int? ExpiresValue { get; set; }
    }
}
