namespace EsolutionPortalDomain.Portal
{
    public class GrupoCotas : EntityBasePortal
    {
        public virtual string? Nome { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual DateTime? DataHoraInclusao { get; set; }
        public virtual int? Usuario { get; set; }
        public virtual int? GrupoTipoSemana { get; set; }

    }
}
