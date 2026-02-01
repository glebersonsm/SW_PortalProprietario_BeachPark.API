namespace CMDomain.Models.SolicitacaoCompraModels
{
    public class SolicitacaoCompraInputModel : ModelRequestBase
    {
        public int? NumSolCompra { get; set; }
        public int? IdPessoa { get; set; }
        public int? IdUsuario { get; set; }
        public int? IdProcesso { get; set; }
        public int? IdReservaOrcamen { get; set; }
        public int? CodAlmoxarifado { get; set; }
        public int? UnidNegoc { get; set; } = -1;
        public string? CodCentroRespon { get; set; } = "9999999999";
        public string? DataEntrega { get; set; }
        public int? IdEmpresa { get; set; }
        public string? CodCentroCusto { get; set; }
        public string? AlgumParaEstoque { get; set; } = null;
        public string? DataEmissao { get; set; }
        public string? SolicitacaoAtendida { get; set; } = "F";
        public string? SolicitacaoAceita { get; set; } = "F";
        public string? CustoEstoque { get; set; } = "E";
        public string? Impresso { get; set; } = "F";
        public string? FlgPrePronta { get; set; } = "N";
        public int? IdContPermuta { get; set; }
        public string? Status { get; set; } = "PE";
        public int? IdArquivo { get; set; }
        public int? IdProcessoSecundario { get; set; }
        public int? IdProcessoMaster { get; set; }
        public string? FlgUrgente { get; set; } = "N";
        public string? FlgWs { get; set; } = "N";
        public string? FlgStatusWs { get; set; } = "N";
        public List<SolicitacaoCompraItemInputModel> Itens { get; set; } = new List<SolicitacaoCompraItemInputModel>();
    }
}
