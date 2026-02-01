namespace CMDomain.Models.Financeiro
{
    public class TipoDocumentoViewModel
    {
        public int? Id { get; set; }
        public string? Descricao { get; set; }
        public string? DebCre { get; set; }
        public string? DocumentoFiscal { get; set; } = "N";
        public string? FlgIntegraFFlex { get; set; } = "N";
        public int? IdModelo { get; set; }
    }
}
