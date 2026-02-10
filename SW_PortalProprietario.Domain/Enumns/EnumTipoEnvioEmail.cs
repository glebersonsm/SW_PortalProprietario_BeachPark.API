namespace SW_PortalProprietario.Domain.Enumns
{
    /// <summary>
    /// Tipo de cliente de envio de e-mail. Em ambos as credenciais vêm dos parâmetros do sistema (ou .env).
    /// </summary>
    public enum EnumTipoEnvioEmail
    {
        /// <summary> Cliente de email direto: MailKit (Connect, Authenticate, SendAsync, Disconnect). </summary>
        ClienteEmailDireto = 0,

        /// <summary> Cliente de email APP: System.Net.Mail (MailMessage + SmtpClient.Send), estilo Climber. </summary>
        ClienteEmailApp = 1,

        /// <summary> AWS SES SMTP: envio via Amazon Simple Email Service (credenciais IAM/SMTP). </summary>
        AwsSes = 2
    }
}
