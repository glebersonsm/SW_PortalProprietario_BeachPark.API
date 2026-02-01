namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class PeriodoCotaDisponibilidadeUsuarioSearchModel : PaginatedSearchModel
    {
        public string? Ano { get; set; }
        public string? SomentePendenteDeReservas { get; set; }
        public int? CotaAcId { get; set; }
        public string? CotaNome { get; set; }
        public string? ImovelNumero { get; set; }
        public DateTime? DataAquisicaoContrato { get; set; }
        public string? IdIntercambiadora { get; set; }
        public string? AgendamentoId { get; set; }
        public string? PadraoDeCor { get; set; } = "Default";
    }
}
