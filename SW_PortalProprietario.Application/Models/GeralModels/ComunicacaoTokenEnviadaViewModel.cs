namespace SW_PortalProprietario.Application.Models.GeralModels
{
    /// <summary>
    /// Registro de comunicação de token 2FA enviada (para listagem e gerenciamento de volume).
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
        public Guid? TwoFactorId { get; set; }
        public int? EmailId { get; set; }
    }

    /// <summary>
    /// Filtros para busca de comunicações de token enviadas.
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
    /// Resumo de volume de comunicações de token por canal e período.
    /// </summary>
    public class ResumoVolumeComunicacaoTokenModel
    {
        public string? Canal { get; set; }
        public int TotalEnviados { get; set; }
        public DateTime? DataHoraEnvioInicial { get; set; }
        public DateTime? DataHoraEnvioFinal { get; set; }
    }
}
