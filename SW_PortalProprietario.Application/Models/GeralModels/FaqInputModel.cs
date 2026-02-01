using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class FaqInputModel : CreateUpdateModelBase
    {
        public virtual int? GrupoFaqId { get; set; }
        public virtual string? Pergunta { get; set; }
        public virtual string? Resposta { get; set; }
        public virtual EnumSimNao? Disponivel { get; set; }
        public virtual int? Ordem { get; set; }
        public bool? RemoverTagsNaoEnviadas { get; set; } = false;
        public List<int>? TagsRequeridas { get; set; }

    }
}
