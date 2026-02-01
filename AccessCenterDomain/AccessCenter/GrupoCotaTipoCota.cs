namespace AccessCenterDomain.AccessCenter
{
    public class GrupoCotaTipoCota : EntityBase
    {
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual int? GrupoCota { get; set; }
        public virtual int? TipoCota { get; set; }
        public virtual int? PrioridadeAgendamentoCota { get; set; }
        public virtual decimal? Percentual { get; set; }
        public virtual string SwVinculosTse { get; set; }

    }
}
