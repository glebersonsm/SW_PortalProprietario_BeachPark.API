namespace CMDomain.Models.Financeiro
{
    public class SearchContaPagarForRequestiaIntegrationModel
    {
        public string? UsuarioLogado { get; set; }
        public int? IdFornecedor { get; set; }
        public List<int> IdDocumentos { get; set; } = new List<int>();
        public int? QuantidadeMaximaRetorno { get; set; } = 1;
        public int? EmpresaId { get; set; }
        public int? TipoDocumento { get; set; }
        public DateTime? DataHoraCriacaoInicial { get; set; }
        public DateTime? DataHoraCriacaoFinal { get; set; }
        public bool? RetornarComErros { get; set; } = false;
        public bool? DesprezarOcsGeradasPeloRecebimentoDaMercadoria { get; set; } = true;

    }
}
