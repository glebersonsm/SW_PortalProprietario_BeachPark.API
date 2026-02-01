using AccessCenterDomain.AccessCenter;
using System.ComponentModel.DataAnnotations;

namespace AccessCenterDomain
{
    public class EntityBaseEsol : IEntityBaseEsol
    {
        [Key]
        public virtual int Id { get; set; }
        public virtual DateTime? DataHoraCriacao { get; set; }
        public virtual int? UsuarioCriacao { get; set; }
        public virtual DateTime? DataHoraAlteracao { get; set; }
        public virtual int? UsuarioAlteracao { get; set; }
        public virtual string? Tag { get; set; }
    }
}
