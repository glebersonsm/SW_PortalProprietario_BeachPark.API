namespace CMDomain.Models.Fornecedor
{
    public class DocumentoInputModel
    {
        public int? IdTipoDocumento { get; set; }
        public string? NumDocumento { get; set; }
        public string? Orgao { get; set; }
        public DateTime? DataEmissao { get; set; }
        public DateTime? DataValidade { get; set; }
        public int? IdEstadoEmissao { get; set; }

    }
}
