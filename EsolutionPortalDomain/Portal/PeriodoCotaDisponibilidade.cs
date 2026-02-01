namespace EsolutionPortalDomain.Portal
{
    public class PeriodoCotaDisponibilidade : EntityBasePortal
    {
        public virtual DateTime? DataInicial { get; set; }
        public virtual DateTime? DataFinal { get; set; }
        public virtual int? UhCondominio { get; set; }
        public virtual DateTime? DataHoraInclusao { get; set; }
        public virtual DateTime? DataHoraExclusao { get; set; }
        public virtual string? TipoDisponibilizacao { get; set; }
        public virtual int? Cota { get; set; }
        public virtual int? UsuarioInclusao { get; set; }
        public virtual int? UsuarioExclusao { get; set; }
        public virtual string? Observacao { get; set; }

    }
}
