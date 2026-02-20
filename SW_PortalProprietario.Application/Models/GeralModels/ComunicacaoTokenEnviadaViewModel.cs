namespace SW_PortalProprietario.Application.Models.GeralModels
{
    /// <summary>
    /// Registro de comunicaÃ§Ã£o de token 2FA enviada (para listagem e gerenciamento de volume).
    /// </summary>
    public class ComunicacaoTokenEnviadaViewModel
    {
        public int Id { get; set; }
        public int? UsuarioId { get; set; }
        public string? Login { get; set; }
        public string? Canal { get; set; }
        public string? Destinatario { get; set; }
        public string? TextoEnviado { get; set; }
        public DateTime DataHoraEnvio { get; set; }
        public string? TwoFactorId { get; set; }
        public int? EmailId { get; set; }
        /// <summary>
        /// Data/hora da primeira abertura do e-mail (quando o destinatÃ¡rio carregou as imagens).
        /// Null se o e-mail nÃ£o foi aberto ou o canal nÃ£o for e-mail.
        /// </summary>
        public DateTime? DataHoraPrimeiraAberturaEmail { get; set; }
    }

    /// <summary>
    /// Filtros para busca de comunicaÃ§Ãµes de token enviadas.
    /// </summary>
    public class SearchComunicacaoTokenEnviadaModel
    {
        public DateTime? DataHoraEnvioInicial { get; set; }
        public DateTime? DataHoraEnvioFinal { get; set; }
        public string? Canal { get; set; }
        public string? Login { get; set; }
        public int? NumeroDaPagina { get; set; }
        public int? QuantidadeRegistrosRetornar { get; set; }
    }

    /// <summary>
    /// Resumo de volume de comunicaÃ§Ãµes de token por canal e perÃ­odo.
    /// </summary>
    public class ResumoVolumeComunicacaoTokenModel
    {
        public string? Canal { get; set; }
        public int TotalEnviados { get; set; }
        public DateTime? DataHoraEnvioInicial { get; set; }
        public DateTime? DataHoraEnvioFinal { get; set; }
    }
}
