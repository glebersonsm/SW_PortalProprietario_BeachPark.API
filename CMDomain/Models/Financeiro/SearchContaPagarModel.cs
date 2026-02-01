namespace CMDomain.Models.Financeiro
{
    public class SearchContaPagarModel
    {
        public string? UsuarioLogado { get; set; }
        public int? IdFornecedor { get; set; }
        public string? NomeFornecedor { get; set; }
        public string? DocumentoFornecedor { get; set; }
        public List<int> IdDocumentos { get; set; } = new List<int>();
        public string? NumeroDocumento { get; set; }
        public DateTime? DataEmissaoInicial { get; set; }
        public DateTime? DataEmissaoFinal { get; set; }
        public DateTime? DataVencimentoInicial { get; set; }
        public DateTime? DataVencimentoFinal { get; set; }
        public int? EmpresaId { get; set; }
        public int? TipoDocumento { get; set; }

    }
}
