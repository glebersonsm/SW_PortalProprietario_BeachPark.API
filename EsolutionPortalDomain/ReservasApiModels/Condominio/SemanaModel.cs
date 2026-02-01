namespace EsolutionPortalDomain.ReservasApiModels.Condominio
{
    public class SemanaModel
    {
        public int? Id { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public int? CotaId { get; set; }
        public string? CotaNome { get; set; }
        public int? UhCondominioId { get; set; }
        public string? UhCondominioNumero { get; set; }
        public string? TipoDisponibilizacao { get; set; }
        public string? tipoDisponibilizacaoNome { get; set; }
        public bool? PodeRetirarDoPool { get; set; }
        public bool? PodeLiberarParaPool { get; set; }

    }
}
