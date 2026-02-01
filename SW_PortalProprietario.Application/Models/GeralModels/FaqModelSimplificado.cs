using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class FaqModelSimplificado : ModelBase
    {
        public virtual GrupoFaqModelSimplificado? GrupoFaq { get; set; }
        public virtual string? Pergunta { get; set; }
        public virtual string? Resposta { get; set; }
        public virtual EnumSimNao? Disponivel { get; set; }
        public virtual int? Ordem { get; set; }
        public List<FaqTagsModel>? TagsRequeridas { get; set; }
    }
}
