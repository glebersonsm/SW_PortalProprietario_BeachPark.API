namespace EsolutionPortalDomain.Portal
{
    public class CotaProprietario : EntityBasePortal
    {
        public virtual int? Cota { get; set; }
        public virtual int? UhCondominio { get; set; }
        public virtual DateTime? DataHoraInclusao { get; set; }
        public virtual int? TipoContrato { get; set; }
        public virtual decimal? ValorTaxaCondominio { get; set; }
        public virtual string? Tag { get; set; }
        public virtual string? BloqueioAgendamentoManual { get; set; } = "N";
        public virtual string? BloqueioManualFinanceiro { get; set; } = "N";

    }
}
