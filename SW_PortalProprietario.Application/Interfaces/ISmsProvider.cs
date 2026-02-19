namespace SW_PortalProprietario.Application.Interfaces
{
    /// <summary>
    /// Proveedor de envio de SMS (ex.: canal Beach Park CXF).
    /// Utilizado para envio do token 2FA por SMS.
    /// </summary>
    public interface ISmsProvider
    {
        /// <summary>
        /// Envia um SMS para o nÃºmero informado com o texto da mensagem.
        /// </summary>
        /// <param name="phoneNumber">NÃºmero do celular (com DDD, apenas dÃ­gitos ou formatado conforme exigido pelo provedor).</param>
        /// <param name="message">Texto da mensagem (ex.: cÃ³digo 2FA).</param>
        /// <param name="baseUrl">URL do endpoint de envio (ex.: do cadastro ParametroSistema). Se null, usa valor de configuraÃ§Ã£o (TwoFactorSms:BaseUrl).</param>
        /// <param name="cancellationToken">Cancelamento.</param>
        Task SendSmsAsync(string phoneNumber, string message, string? baseUrl = null, CancellationToken cancellationToken = default);
    }
}
