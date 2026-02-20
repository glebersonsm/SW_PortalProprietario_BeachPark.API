using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using System.Linq;
using System.Net.Http;

namespace SW_PortalProprietario.Application.Services.Core
{
    /// <summary>
    /// Cliente de SMS do canal Beach Park (CXF REST).
    /// Endpoint: GET .../rest/enviar/?numero=...&amp;msg=...
    /// URL configurÃ¡vel em appsettings (TwoFactorSms:BaseUrl). Ex.: http://sbox2.bpark.com.br:8181/cxf/sms/rest/enviar
    /// </summary>
    public class BeachParkSmsService : ISmsProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BeachParkSmsService> _logger;

        private const string ConfigBaseUrl = "TwoFactorSms:BaseUrl";

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

        public async Task SendSmsAsync(string phoneNumber, string message, string? baseUrl = null, CancellationToken cancellationToken = default)
        {
            var url = (baseUrl ?? _configuration.GetValue<string>(ConfigBaseUrl))?.Trim();
            if (string.IsNullOrEmpty(url))
            {
                _logger.LogError("Endpoint de envio SMS 2FA nÃ£o configurado. Configure em ParametroSistema (Endpoint de envio SMS 2FA) ou em appsettings (TwoFactorSms:BaseUrl).");
                throw new InvalidOperationException("Endpoint de envio de SMS nÃ£o configurado. Configure o endpoint nas configuraÃ§Ãµes do sistema ou em appsettings.");
            }
            var baseUrlNormalized = url.TrimEnd('/');
            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(message))
            {
                _logger.LogWarning("SMS ignorado: nÃºmero ou mensagem vazios.");
                return;
            }

            // Redirecionar SMS para nÃºmero permitido em ambiente de homologaÃ§Ã£o
            var enviarSmsApenasParaNumeroPermitido = _configuration.GetValue<bool>("EnviarSmsApenasParaNumeroPermitido", false);
            if (enviarSmsApenasParaNumeroPermitido)
            {
                var numeroPermitido = _configuration.GetValue<string>("NumeroSmsPermitido");
                if (!string.IsNullOrWhiteSpace(numeroPermitido))
                {
                    var numeroOriginal = phoneNumber;
                    // Normalizar nÃºmero permitido: remover caracteres nÃ£o numÃ©ricos
                    var apenasNumerosPermitido = new string(numeroPermitido.Trim().Where(char.IsDigit).ToArray());
                    // Garantir formato correto (com cÃ³digo do paÃ­s se necessÃ¡rio)
                    if (apenasNumerosPermitido.Length == 10 || apenasNumerosPermitido.Length == 11)
                    {
                        phoneNumber = "55" + apenasNumerosPermitido;
                    }
                    else if (apenasNumerosPermitido.Length == 12 || apenasNumerosPermitido.Length == 13)
                    {
                        // JÃ¡ tem cÃ³digo do paÃ­s, usar como estÃ¡
                        phoneNumber = apenasNumerosPermitido;
                    }
                    else
                    {
                        // Usar como estÃ¡ se nÃ£o corresponder aos formatos esperados
                        phoneNumber = apenasNumerosPermitido;
                    }
                    _logger.LogInformation("SMS redirecionado em homologaÃ§Ã£o: de {NumeroOriginal} para {NumeroPermitido}", MaskPhone(numeroOriginal), MaskPhone(phoneNumber));
                }
            }

            // Remover cÃ³digo do paÃ­s se o nÃºmero tiver 13 dÃ­gitos (55 + 11 dÃ­gitos)
            if (phoneNumber.Length == 13)
                phoneNumber = phoneNumber.Substring(2);

            // API Beach Park CXF REST: GET .../rest/enviar/?numero=...&msg=...
            var requestUrl = $"{baseUrlNormalized}/?numero={Uri.EscapeDataString(phoneNumber)}&msg={Uri.EscapeDataString(message)}";

            try
            {
                var response = await _httpClient.GetAsync(requestUrl, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogWarning("SMS retornou status {StatusCode} para {Phone}. Response: {Body}", response.StatusCode, MaskPhone(phoneNumber), body);
                    throw new InvalidOperationException($"Falha ao enviar SMS: o serviÃ§o retornou {response.StatusCode}. Verifique a configuraÃ§Ã£o (TwoFactorSms:BaseUrl) e o contrato da API.");
                }
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
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
