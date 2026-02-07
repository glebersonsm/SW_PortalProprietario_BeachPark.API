namespace SW_PortalProprietario.Application.Interfaces
{
    /// <summary>
    /// Proveedor de envio de SMS (ex.: canal Beach Park CXF).
    /// Utilizado para envio do token 2FA por SMS.
    /// </summary>
    public interface ISmsProvider
    {
        /// <summary>
        /// Envia um SMS para o número informado com o texto da mensagem.
        /// </summary>
        /// <param name="phoneNumber">Número do celular (com DDD, apenas dígitos ou formatado conforme exigido pelo provedor).</param>
        /// <param name="message">Texto da mensagem (ex.: código 2FA).</param>
        /// <param name="cancellationToken">Cancelamento.</param>
        Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    }
}
