namespace CMDomain.Models.Fornecedor
{
    public class DocumentoUpdateInputModel : ModelRequestBase
    {
        public int? IdTipoDocumento { get; set; }
        public int? IdFornecedor { get; set; }
        public int? IdEmpresa { get; set; }
        public string? NumDocumento { get; set; }
        public string? Orgao { get; set; }
        public DateTime? DataEmissao { get; set; }
        public DateTime? DataValidade { get; set; }
        public int? IdEstadoEmissao { get; set; }

    }
}
