using SW_Utils.Interfaces;

namespace SW_PortalProprietario.Application.Models
{
    /// <summary>
    /// Modelo de dados para geração de incentivo para agendamento
    /// </summary>
    public class IncentivoParaAgendamentoEmailDataModel
    {
        public string? HtmlContent { get; set; }
        public string? Subject { get; set; }
        public byte[]? PdfBytes { get; set; }
        public string? PdfFileName { get; set; }
        public int DaysBefore { get; set; }
        public dynamic? DadosBaseEnvio { get; set; }
    }
}
