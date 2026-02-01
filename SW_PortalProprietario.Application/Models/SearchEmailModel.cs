using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models
{
    public class SearchEmailModel
    {
        public int? Id { get; set; }
        public DateTime? DataHoraCriacaoInicial { get; set; }
        public DateTime? DataHoraCriacaoFinal { get; set; }
        public DateTime? DataHoraEnvioInicial { get; set; }
        public DateTime? DataHoraEnvioFinal { get; set; }
        public EnumSimNao? Enviado { get; set; }
        public string? Destinatario { get; set; }
        public string? Assunto { get; set; }
        public int? NumeroDaPagina { get; set; }
        public int? QuantidadeRegistrosRetornar { get; set; }

    }
}
