using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class DocumentGroupInputModel : CreateUpdateModelBase
    {
        public string? Nome { get; set; }
        public EnumSimNao? Disponivel { get; set; }
        public EnumSimNao? GrupoPublico { get; set; }
        public int? Ordem { get; set; }
        public bool? RemoverTagsNaoEnviadas { get; set; } = false;
        public List<int>? TagsRequeridas { get; set; }

    }
}
