using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class AlteracaoDocumentInputModel : CreateUpdateModelBase
    {
        public string? Nome { get; set; }
        public EnumSimNao? DocumentoPublico { get; set; }
        public EnumSimNao? Disponivel { get; set; }
        public DateTime? DataInicioVigencia { get; set; }
        public DateTime? DataFimVigencia { get; set; }
        public bool? RemoverTagsNaoEnviadas { get; set; } = false;
        public List<int>? TagsRequeridas { get; set; }
        public string? Cor { get; set; }
        public string? CorTexto { get; set; }

    }
}
