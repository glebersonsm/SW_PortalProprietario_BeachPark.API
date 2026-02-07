using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SW_PortalProprietario.Application.Services.Core
{
    /// <summary>
    /// Cliente de SMS do canal Beach Park (CXF).
    /// URL configurável em appsettings (ex.: TwoFactorSms:BaseUrl).
    /// Contrato (método, parâmetros, autenticação) deve ser ajustado conforme WADL em ?_wadl.
    /// </summary>
    public class BeachParkSmsService : ISmsProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BeachParkSmsService> _logger;

        private const string ConfigBaseUrl = "TwoFactorSms:BaseUrl";
        private const string DefaultSmsUrl = "http://sbox2.bpark.com.br:8181/cxf/sms";

        public BeachParkSmsService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<BeachParkSmsService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        public async Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            var baseUrl = _configuration.GetValue<string>(ConfigBaseUrl)?.TrimEnd('/') ?? DefaultSmsUrl;
            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(message))
            {
                _logger.LogWarning("SMS ignorado: número ou mensagem vazios.");
                return;
            }

            // Contrato CXF pode exigir SOAP ou REST com parâmetros específicos; ajustar conforme WADL.
            // Assumindo POST com form-urlencoded (numero, mensagem) como placeholder.
            var form = new Dictionary<string, string>
            {
                ["numero"] = phoneNumber,
                ["mensagem"] = message
            };
            using var content = new FormUrlEncodedContent(form);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            try
            {
                var response = await _httpClient.PostAsync(baseUrl, content, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning("SMS retornou status {StatusCode} para {Phone}", response.StatusCode, MaskPhone(phoneNumber));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar SMS para {Phone}", MaskPhone(phoneNumber));
                throw;
            }
        }

        private static string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 4) return "***";
            return phone.Length <= 6 ? "****" + phone[^2..] : "*****" + phone[^4..];
        }
    }
}
