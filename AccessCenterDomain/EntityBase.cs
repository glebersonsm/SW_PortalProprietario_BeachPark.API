namespace AccessCenterDomain
{
    public class EntityBase
    {
        public virtual int? Id { get; set; }
        public virtual DateTime? DataHoraCriacao { get; set; }
        public virtual int? UsuarioCriacao { get; set; }
        public virtual DateTime? DataHoraAlteracao { get; set; }
        public virtual int? UsuarioAlteracao { get; set; }
        public virtual string? Tag { get; set; }
    }
}
