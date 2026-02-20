using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class DocumentoModelSimplificado : ModelBase
    {
        public int? GrupoDocumentoId { get; set; }
        public string? Nome { get; set; }
        public byte[]? Arquivo { get; set; }
        public string? NomeArquivo { get; set; }
        public string? TipoMime { get; set; }
        public EnumSimNao? Disponivel { get; set; }
        public int? Ordem { get; set; }
        public DateTime? DataInicioVigencia { get; set; }
        public string? DataInicioVigenciaStr => DataInicioVigencia?.ToString("yyyy-MM-dd");
        public DateTime? DataFimVigencia { get; set; }
        public string? DataFimVigenciaStr => DataFimVigencia?.ToString("yyyy-MM-dd");

        public List<DocumentoTagsModel>? TagsRequeridas { get; set; }
        public string? Cor { get; set; }
        public string? CorTexto { get; set; }

    }
}
