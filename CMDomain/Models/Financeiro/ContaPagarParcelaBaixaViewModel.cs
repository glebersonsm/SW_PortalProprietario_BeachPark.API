
namespace CMDomain.Models.Financeiro
{
    public class ContaPagarParcelaBaixaViewModel
    {
        /// <summary>
        /// IdDocumento chave primária composta com o campo NumeroLancamento
        /// </summary>
        public int? IdDocumento { get; set; }
        /// <summary>
        /// NumeroLancamento chave primária composta com o campo IdDocumento
        /// </summary>
        public int? NumeroLancamento { get; set; }
        /// <summary>
        /// ValorBaixado
        /// </summary>
        public decimal? ValorBaixado { get; set; }
        /// <summary>
        /// Data da baixa
        /// </summary>
        public DateTime? DataBaixa { get; set; }
        public string? HistoricoComplementar { get; set; }
        public int? Usuario { get; set; }
        public int? TipoPagamentoId { get; set; }
        /// <summary>
        /// Codigo da forma de pagamento
        /// </summary>
        public int? CodFormaPagamento { get; set; }
        /// <summary>
        /// Nome da forma de pagamento
        /// </summary>
        public string? NomeFormaPagamento { get; set; }
        public string? NomeUsuario { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }

    }
}
