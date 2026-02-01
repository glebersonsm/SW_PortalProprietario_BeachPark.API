namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class DocumentoHistoricoModel
    {
        public int? GrupoDocumentoId { get; set; }
        public string? GrupoDocumentoNome { get; set; }
        public int? DocumentoId { get; set; }
        public string? DocumentoNome { get; set; }
        public string? DocumentoPath { get; set; }
        public string? AcaoRealizada { get; set; }
        public int? UsuarioId { get; set; }
        public string? NomeUsuario { get; set; }
        public DateTime? DataOperacao { get; set; }

    }
}
