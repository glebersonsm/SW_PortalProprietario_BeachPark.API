namespace SW_PortalProprietario.Application.Models.UsuarioFinanceiro
{
    public class ContaPendenteModel
    {
        public int Id { get; set; }
        public int? BoletoId { get; set; }
        public string? Contrato { get; set; }
        public string? StatusParcela { get; set; }
        public DateTime? DataHoraCriacao { get; set; }
        public int? EmpresaId { get; set; }
        public string? EmpresaNome { get; set; }
        public int? PessoaProviderId { get; set; }
        public int? PessoaId { get; set; }
        public int? ClienteId { get; set; }
        public string? NomePessoa { get; set; }
        public DateTime? DataCriacao { get; set; }
        public DateTime? Vencimento { get; set; }
        public string? CodigoTipoConta { get; set; }
        public string? NomeTipoConta { get; set; }
        public decimal? Valor { get; set; }
        public decimal? Saldo { get; set; }
        public string? LinhaDigitavelBoleto { get; set; }
        public string? NossoNumeroBoleto { get; set; }
        public string? Observacao { get; set; }
        public string? EmpreendimentoCnpj { get; set; }
        public string? EmpreendimentoNome { get; set; }
        public int? PessoaEmpreendimentoId { get; set; }
        public string? NumeroImovel { get; set; }
        public string? NumeroContrato { get; set; }
        public string? DocumentoCliente { get; set; }
        public string? FracaoCota { get; set; }
        public string? BlocoCodigo { get; set; }
        public DateTime? LimitePagamentoTransmitido { get; set; }
        public string? ComLimitePagamentoTra { get; set; }
        public string? ComLimitePagamento { get; set; }
        public decimal? ValorJuroDiario { get; set; }
        public decimal? PercentualJuroDiario { get; set; }
        public decimal? PercentualJuroDiarioCar { get; set; }
        public decimal? PercentualJuroMensal { get; set; }
        public decimal? ValorJuroMensal { get; set; }
        public decimal? PercentualMulta { get; set; }
        public decimal? PercentualMultaCar { get; set; }
        public decimal? ValorAtualizado { get; set; }
        public decimal? TaxaJuroMensalProcessamento { get; set; }
        public decimal? TaxaMultaMensalProcessamento { get; set; }
        public DateTime? DataBaseAplicacaoJurosMultas { get; set; }
        public string? PodeAplicarMulta { get; set; }
        public DateTime? DataHoraBaixa { get; set; }
        public string? StatusCrcBloqueiaPagamento { get; set; } = "N";
        public DateTime? DataProcessamentoCartaoRec { get; set; }
        public DateTime? DataProcessamento { get; set; }

    }
}
