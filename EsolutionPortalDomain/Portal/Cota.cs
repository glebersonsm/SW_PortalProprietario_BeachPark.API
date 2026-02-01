namespace EsolutionPortalDomain.Portal
{
    public class Cota : EntityBasePortal
    {
        public virtual string? Nome { get; set; }
        public virtual int? GrupoCotas { get; set; }
        public virtual decimal? Percentagem { get; set; }
        public virtual int? UhCondominio { get; set; }
        public virtual int? TipoCota { get; set; }
        public virtual int? PrioridadeAgendamento { get; set; }

    }
}
