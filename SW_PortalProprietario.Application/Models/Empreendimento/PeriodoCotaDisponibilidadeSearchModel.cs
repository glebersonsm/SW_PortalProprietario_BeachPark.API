namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    /// <summary>
    /// Modelo de busca para consulta geral de semanas - migrado do SwReservaApiMain.
    /// </summary>
    public class PeriodoCotaDisponibilidadeSearchModel : PaginatedSearchModel
    {
        public int? CotaProprietarioId { get; set; }
        public int? PeriodoCotaDisponibilidadeId { get; set; }
        public string? DocumentoProprietario { get; set; }
        public string? NomeProprietario { get; set; }
        public string? NumeroApartamento { get; set; }
        public int? Ano { get; set; }
        public DateTime? CheckinInicial { get; set; }
        public DateTime? CheckinFinal { get; set; }
        public DateTime? CheckoutInicial { get; set; }
        public DateTime? CheckoutFinal { get; set; }
        public DateTime? DataUtilizacaoInicial { get; set; }
        public DateTime? DataUtilizacaoFinal { get; set; }
        public int? Reserva { get; set; }
        public string? ComReservas { get; set; }
        public string? NomeCota { get; set; }
        public DateTime? DataAquisicaoContrato { get; set; }
        public List<string>? NumeroApartamentos { get; set; }
        public List<string>? NomeCotas { get; set; }
    }
}
