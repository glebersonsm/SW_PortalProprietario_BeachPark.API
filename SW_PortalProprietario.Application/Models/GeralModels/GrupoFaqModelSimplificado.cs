using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class GrupoFaqModelSimplificado : ModelBase
    {
        public virtual int? EmpresaId { get; set; }
        public virtual string? Nome { get; set; }
        public virtual EnumSimNao? Disponivel { get; set; }

    }
}
