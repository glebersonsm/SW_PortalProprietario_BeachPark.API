namespace EsolutionPortalDomain.Portal
{
    public class GrupoTipoSemana : EntityBasePortal
    {
        public virtual string? Nome { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual DateTime? DataHoraCriacao { get; set; }
        public virtual int? UsuarioCriacao { get; set; }
        public virtual DateTime? DataHoraExclusao { get; set; }
        public virtual int? UsuarioExlcusao { get; set; }

    }
}
