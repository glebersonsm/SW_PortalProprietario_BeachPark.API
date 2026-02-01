

using CMDomain.Entities;

namespace CMDomain.Models.Cotacao
{
    public class CotacoesItemViewModel
    {
        public int? CodProcesso { get; set; }
        public int? IdProcXArt { get; set; }
        public int? IdFornecedor { get; set; }
        public string? NomeFornecedor { get; set; }
        public string? CpfCnpjFornecedor { get; set; }
        public int? Proposta { get; set; }
        public decimal? Quantidade { get; set; }
        public int? IdItemOc { get; set; }
        public string? CodMedida { get; set; }
        public string? Status { get; set; }
        public string? Observacao { get; set; }
        public string? Contato { get; set; }
        public decimal? Preco { get; set; }
        public string Ganhador => IdItemOc.GetValueOrDefault(0) > 0 ? "Sim" : "Não";
        public string? Justificativa { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }
        public List<CotacaoItemPrecificacaoCustoAgregViewModel> CustosAgregados { get; set; } = new List<CotacaoItemPrecificacaoCustoAgregViewModel>();
        public List<CotacaoCustoAgregNotaViewModel> CustoAgregadoCotacao { get; set; } = new List<CotacaoCustoAgregNotaViewModel>();
        public List<CotacaoItemPrecificacaoEntregasViewModel> Entregas { get; set; } = new List<CotacaoItemPrecificacaoEntregasViewModel>();
        public List<CotacaoItemPrecificacaoPrazoPagamentoViewModel> PrazoPagamento { get; set; } = new List<CotacaoItemPrecificacaoPrazoPagamentoViewModel>();


        public static explicit operator CotacoesItemViewModel(Cotacoes model)
        {
            return new CotacoesItemViewModel
            {
                CodProcesso = model.CodProcesso,
                IdProcXArt = model.IdProcXArt,
                IdFornecedor = model.IdForCli,
                Proposta = model.Proposta,
                Quantidade = model.QtdeFornecida,
                IdItemOc = model.IdItemOc,
                CodMedida = model.CodMedida,
                Status = model.Status,
                Observacao = model.Obs,
                Preco = model.Preco,
                Contato = model.Contato,
                TrgUserInclusao = model.TrgUserInclusao,
                TrgDtInclusao = model.TrgDtInclusao
            };
        }
    }
}
