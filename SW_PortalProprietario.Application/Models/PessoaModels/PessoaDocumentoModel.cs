namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class PessoaDocumentoModel : ModelBase
    {
        public PessoaDocumentoModel()
        { }

        public int? PessoaId { get; set; }
        public int? TipoDocumentoId { get; set; }
        public string? TipoDocumentoNome { get; set; }
        public string? Numero { get; set; }
        public string? ValorNumerico { get; set; }
        public string? NumeroFormatado { get; set; }
        public string? TipoDocumentoMascara { get; set; }
        public string? OrgaoEmissor { get; set; }
        public DateTime? DataEmissao { get; set; }
        public DateTime? DataValidade { get; set; }

    }
}

