namespace SW_PortalProprietario.Infra.Data.Mappings.Core
{
    public class EntityBaseCore : IEntityBaseCore
    {
        public virtual int Id { get; set; }
        public virtual DateTime? DataHoraCriacao { get; set; }
        public virtual int? UsuarioCriacao { get; set; }
        public virtual DateTime? DataHoraAlteracao { get; set; }
        public virtual int? UsuarioAlteracao { get; set; }
        public virtual string? ObjectGuid { get; set; }
    }
}
