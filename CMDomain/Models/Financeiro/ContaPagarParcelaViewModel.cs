namespace CMDomain.Models.Financeiro
{
    public class ContaPagarParcelaViewModel
    {
        /// <summary>
        /// IdDocumento chave primária composta juntamente com o campo NumeroLancamento que representa uma parcela de um documento
        /// </summary>
        public int? IdDocumento { get; set; }
        public int? Parcela { get; set; } = 1;
        /// <summary>
        /// NumeroLancamento chave primária composta juntamente com o campo IdDocumento que representa uma parcela de um documento
        /// </summary>
        public int? NumeroLancamento { get; set; }
        public decimal? ValorOriginal { get; set; }
        public decimal? ValorAtual => ValorOriginal + AlteradoresDeValores.Sum(c => c.ValorAlteracao);
        public decimal? ValorBaixado => Baixas.Sum(c => c.ValorBaixado);
        public decimal? SaldoPendente => ValorAtual + (ValorBaixado > 0 ? (ValorBaixado * (-1)) : ValorBaixado);
        public string? Status => (SaldoPendente == 0.00m || DataBaixa.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue) ? "Baixado" : "Pendente";
        public DateTime? DataBaixa => Baixas.Any() ? Baixas.Last().DataBaixa : null;
        public DateTime? DataLancamento { get; set; }
        public string? HistoricoComplemnetar { get; set; }
        public int? Usuario { get; set; }
        public string? NomeUsuario { get; set; }
        public int? IdContaBancariaXChavePix { get; set; }
        public string? ChavePix { get; set; }
        public int? IdTipoChave { get; set; }
        public string? TipoChavePix { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }
        public List<ContaPagarParcelaAlteradorValorViewModel> AlteradoresDeValores { get; set; } = new List<ContaPagarParcelaAlteradorValorViewModel>();
        public List<ContaPagarParcelaBaixaViewModel> Baixas { get; set; } = new List<ContaPagarParcelaBaixaViewModel>();


    }
}
