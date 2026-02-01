namespace CMDomain.Models.ProdutoModels
{
    public class TributacaoProdutoInputModel
    {
        public string? SituacaoTributariaParteA { get; set; }
        public string? SituacaoTributariaParteB { get; set; }
        public string? SituacaoTribSaida { get; set; }
        public string? SituacaoTributariaPis { get; set; }
        public string? SituacaoTributariaCofins { get; set; }
        public string? SituacaoTributariaPisSaida { get; set; } = "01";
        public string? SituacaoTributariaCofinsSaida { get; set; } = "01";
        public string? RegraBaseCalculoPisCofins { get; set; } = "P";//R = Receita proporcional, P = Valor integral, N = Isento ou não incidência
        public string? CodStIpi { get; set; } = "03";
        public string? ConsumoRevenda { get; set; } = "R";
        public string? CodStIpiSaida { get; set; } = "99";
        public string? CodFiscalPadrao { get; set; }
        public string? CodTipoItemEstoque { get; set; }
        public string? IsentoOutros { get; set; } = "O";
        public string? ProprietarioDoItem { get; set; } = "0"; //0 = Propriedade do informante ou em seu poder, 1 = Conselheiro de Administração do Informante e em posso de Terceiros, 2 = Propriedade de terceiros em posse do informante
        public bool? IncluirEmTodasAsEmpresas { get; set; } = true;
        public List<int> EmpresaIncluirId { get; set; } = new List<int>();
        public string? ProdutoOrigemCopiar { get; set; }

    }
}
