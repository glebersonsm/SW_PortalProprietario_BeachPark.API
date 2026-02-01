namespace CMDomain.Models.ProdutoModels
{
    public class TributacaoProdutoViewModel
    {

        public int? IdProdutoXEmpresa { get; set; }
        public int? IdEmpresa { get; set; }
        public string? CodProduto { get; set; }
        public string? SituacaoTributariaParteA { get; set; }
        public string? SituacaoTributariaParteB { get; set; }
        public string? SituacaoTributariaPis { get; set; }
        public string? SituacaoTributariaCofins { get; set; }
        public string? SituacaoTributariaPisSaida { get; set; }
        public string? SituacaoTributariaCofinsSaida { get; set; }
        public string? RegraBaseCalculoPisCofins { get; set; } = "P";//R = Receita proporcional, P = Valor integral, N = Isento ou não incidência
        public string? SituacaoTributariaSaida { get; set; }
        public string? CodStIpi { get; set; }
        public string? CodStIpiSaida { get; set; }
        public string? CodFiscalPadrao { get; set; }
        public string? CodTipoItemEstoque { get; set; }
        public string? DescricaoTipoItemEstoque { get; set; }
        public string? IsentoOutros { get; set; } = "O";//O = Outros, I = Isento
        public string? ProprietarioDoItem { get; set; } = "0"; //0 = Propriedade do informante ou em seu poder, 1 = Conselheiro de Administração do Informante e em posso de Terceiros, 2 = Propriedade de terceiros em posse do informante
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }

    }
}
