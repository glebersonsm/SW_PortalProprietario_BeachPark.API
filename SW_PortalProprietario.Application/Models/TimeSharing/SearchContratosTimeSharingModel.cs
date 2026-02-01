namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class SearchContratosTimeSharingModel
    {
        public string? NomeCliente { get; set; }
        public string? NumeroContrato { get; set; }
        public string? NumDocumentoCliente { get; set; }
        public string? TipoContrato { get; set; }
        public string? ProjetoXContrato { get; set; }
        public string? Status { get; set; }
        public DateTime? DataVendaInicial { get; set; }
        public DateTime? DataVendaFinal { get; set; }
        public DateTime? DataCancelamentoInicial { get; set; }
        public DateTime? DataCancelamentoFinal { get; set; }
        public int? IdVendaTs { get; set; }
        public string? SalaVendas { get; set; }
        public int? NumeroDaPagina { get; set; }
        public int? QuantidadeRegistrosRetornar { get; set; }

    }
}
