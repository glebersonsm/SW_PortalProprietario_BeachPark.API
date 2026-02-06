using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class GrupoDocumentoModel : ModelBase
    {
        public int? EmpresaId { get; set; }
        public string? Nome { get; set; }
        public EnumSimNao? Disponivel { get; set; }
        public int? Ordem { get; set; }
        public List<GrupoDocumentoTagsModel>? TagsRequeridas { get; set; }
        public List<DocumentoModelSimplificado>? Documentos { get; set; }
        public int? GrupoDocumentoPaiId { get; set; }
        public GrupoDocumentoModel? Parent { get; set; }
    }
}
