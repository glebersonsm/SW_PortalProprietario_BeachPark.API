using SW_PortalProprietario.Domain.Entities.Core;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    /// <summary>
    /// Registro de toda comunicação de token 2FA enviada (e-mail ou SMS).
    /// Permite auditoria e gerenciamento de volume de mensagens enviadas pelo portal.
    /// </summary>
    public class ComunicacaoTokenEnviada : EntityBaseCore
    {
        public virtual Usuario? Usuario { get; set; }
        /// <summary> Login do usuário no momento do envio (para consultas e relatórios). </summary>
        public virtual string? Login { get; set; }
        /// <summary> Canal: "email" ou "sms". </summary>
        public virtual string? Canal { get; set; }
        /// <summary> Destinatário (e-mail ou número de telefone utilizado). </summary>
        public virtual string? Destinatario { get; set; }
        /// <summary> Texto completo enviado (corpo do e-mail ou mensagem SMS). </summary>
        public virtual string? TextoEnviado { get; set; }
        /// <summary> Data/hora em que a mensagem foi enviada. </summary>
        public virtual DateTime DataHoraEnvio { get; set; }
        /// <summary> Id da sessão 2FA no cache (para rastreio). </summary>
        public virtual Guid? TwoFactorId { get; set; }
        /// <summary> Id do registro na tabela Email, quando canal = email. </summary>
        public virtual int? EmailId { get; set; }
    }
}
