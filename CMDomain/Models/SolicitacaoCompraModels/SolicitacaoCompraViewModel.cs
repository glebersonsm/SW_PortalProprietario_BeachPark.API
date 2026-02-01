


using CMDomain.Entities;

namespace CMDomain.Models.SolicitacaoCompraModels
{
    public class SolicitacaoCompraViewModel
    {
        public int? NumSolCompra { get; set; }
        public int? IdPessoa { get; set; }
        public int? IdUsuario { get; set; }
        public int? NumRequisicao { get; set; }
        public int? IdProcesso { get; set; }
        public int? IdReservaOrcamen { get; set; }
        public int? CodAlmoxarifado { get; set; }
        public int? UnidNegoc { get; set; } = -1;
        public string? CodCentroResp { get; set; }
        public DateTime? DataEntrega { get; set; }
        public int? IdEmpresa { get; set; }
        public string? CodCentroCusto { get; set; }
        public string? AlgumParaEstoque { get; set; }
        public DateTime? DataEmissao { get; set; }
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
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }

        public List<SolicitacaoCompraItemViewModel> Itens { get; set; } = new List<SolicitacaoCompraItemViewModel>();

        public static explicit operator SolicitacaoCompraViewModel(SoliComp model)
        {
            return new SolicitacaoCompraViewModel
            {
                NumSolCompra = model.NumSolCompra,
                IdPessoa = model.IdPessoa,
                IdUsuario = model.IdUsuario,
                NumRequisicao = model.NumRequisicao,
                IdProcesso = model.IdProcesso,
                IdReservaOrcamen = model.IdReservaOrcamen,
                CodAlmoxarifado = model.CodAlmoxarifado,
                UnidNegoc = model.UnidNegoc,
                CodCentroResp = model.CodCentroRespon,
                DataEntrega = model.DataEntrega,
                IdEmpresa = model.IdEmpresa,
                CodCentroCusto = model.CodCentroCusto,
                AlgumParaEstoque = model.AlgumParaEstoque,
                DataEmissao = model.DataEmissao,
                SolicitacaoAtendida = model.SoliciAtendida,
                SolicitacaoAceita = model.SoliciAceita,
                CustoEstoque = model.CustoEstoque,
                Impresso = model.Impresso,
                Status = model.Status,
                FlgPrePronta = model.FlgPrePronta,
                IdContPermuta = model.IdContPermuta,
                IdArquivo = model.IdArquivo,
                IdProcessoMaster = model.IdProcessoMaster,
                IdProcessoSecundario = model.IdProcessoSecundario,
                FlgUrgente = model.FlgUrgente,
                TrgUserInclusao = model.TrgUserInclusao,
                TrgDtInclusao = model.TrgDtInclusao
            };
        }
    }
}
