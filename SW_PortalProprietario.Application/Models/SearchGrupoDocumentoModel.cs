namespace SW_PortalProprietario.Application.Models
{
    public class SearchGrupoDocumentoModel : SearchPadraoModel
    {
        public bool? RetornarDocumentosDoGrupo { get; set; } = false;
        public int? IdGrupoDocumentoPai { get; set; }

    }
}
