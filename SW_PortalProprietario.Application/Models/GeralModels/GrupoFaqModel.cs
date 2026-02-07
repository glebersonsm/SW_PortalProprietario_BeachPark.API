using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class GrupoFaqModel : ModelBase
    {
        public virtual int? EmpresaId { get; set; }
        public virtual string? Nome { get; set; }
        public virtual EnumSimNao? Disponivel { get; set; }
        public virtual int? Ordem { get; set; }
        public int? GrupoFaqPaiId { get; set; }
        public GrupoFaqModel? Parent { get; set; }
        public List<FaqModelSimplificado>? Faqs { get; set; }
        public List<GrupoFaqTagsModel>? TagsRequeridas { get; set; }

    }
}
