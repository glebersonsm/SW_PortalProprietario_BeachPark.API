namespace EsolutionPortalDomain.Portal
{
    public class PeriodoCotaDisponibilidadePool : EntityBasePortal
    {
        public virtual DateTime? DataHoraInclusao { get; set; }
        public virtual DateTime? DataHoraExclusao { get; set; }
        public virtual int? PeriodoCotaDisponibilidade { get; set; }
        public virtual int? UsuarioInclusao { get; set; }
        public virtual int? UsuarioExclusao { get; set; }

    }
}
