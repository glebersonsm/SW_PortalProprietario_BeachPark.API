
using CMDomain.Entities;
using System.Text.Json.Serialization;

namespace CMDomain.Models.Cotacao
{
    public class SumarioCotacaoItemViewModel
    {
        public int? CodProcesso { get; set; }
        public int? IdProcXArt { get; set; }
        public int? IdFornecedor { get; set; }
        public string? NomeFornecedor { get; set; }
        public string? CpfCnpjFornecedor { get; set; }
        public int? Proposta { get; set; }
        public string? CodMedida { get; set; }
        public string? Status { get; set; }
        public decimal? Preco { get; set; }
        public string? CodigoProduto { get; set; }
        public string? NomeProduto { get; set; }
        public decimal? QuantidadePedida { get; set; }
        public decimal? QuantidadeFornecida { get; set; }
        public string? Contato { get; set; }
        public string? CodigoGrupoProduto { get; set; }
        public string? NomeGrupoProduto { get; set; }

        public string? Justificativa { get; set; }
        [JsonIgnore]
        public int Pontuacao { get; set; } = 0;
        [JsonIgnore]
        public Cotacoes? Cotacao { get; set; }

        public decimal? CustoAgregadoDaNota => CustosAgregados.Where(c => c.TipoCalculo == "TotalNota" && c.Valor.GetValueOrDefault(0.00m) > 0.00m).Sum(c => c.Valor) +
            CustosAgregados.Where(c => c.TipoCalculo == "TotalNota" && c.Percentual.GetValueOrDefault(0.00m) > 0.00m && c.BaseCalculo.GetValueOrDefault(0.00m) > 0.00m).Sum(c => c.BaseCalculo * c.Valor / 100);
        public decimal? PrecoUnitarioTotal => (Preco.GetValueOrDefault(0.00m) > 0.00m && QuantidadeFornecida.GetValueOrDefault(0.00m) > 0.00m ?
            (Preco.GetValueOrDefault(0.00m) +
            (CustosAgregados.Where(c => c.TipoCalculo == "TotalDoItem" && c.Valor.GetValueOrDefault(0.00m) > 0.00m && c.Percentual.GetValueOrDefault(0.00m) == 0.00m).Sum(c => c.Valor) +
            CustosAgregados.Where(c => c.TipoCalculo == "TotalDoItem" && c.Percentual.GetValueOrDefault(0) > 0 && c.Valor.GetValueOrDefault(0.00m) == 0.00m).Sum(c => (c.Percentual.GetValueOrDefault(0.00m) * Preco.GetValueOrDefault(0.00m) / 100)))) : 0.00m);

        public decimal PrecoTotal => Math.Round(QuantidadeFornecida.GetValueOrDefault(0.00m) * PrecoUnitarioTotal.GetValueOrDefault(0.00m), 2);

        public List<CotacaoItemPrecificacaoCustoAgregViewModel> CustosAgregados { get; set; } = new List<CotacaoItemPrecificacaoCustoAgregViewModel>();
        public List<CotacaoItemPrecificacaoEntregasViewModel> Entregas { get; set; } = new List<CotacaoItemPrecificacaoEntregasViewModel>();
        public List<CotacaoItemPrecificacaoPrazoPagamentoViewModel> PrazoPagamento { get; set; } = new List<CotacaoItemPrecificacaoPrazoPagamentoViewModel>();


    }
}
