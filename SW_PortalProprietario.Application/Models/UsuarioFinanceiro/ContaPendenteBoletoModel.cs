namespace SW_PortalProprietario.Application.Models.UsuarioFinanceiro
{
    public class ContaPendenteBoletoModel
    {
        public int Id { get; set; }
        public DateTime? DataHoraCriacao { get; set; }
        public int? PessoaProviderId { get; set; }
        public int? PessoaId { get; set; }
        public string? NomePessoa { get; set; }
        public DateTime? DataCriacao { get; set; }
        public DateTime? Vencimento { get; set; }
        public string? CodigoTipoConta { get; set; }
        public string? NomeTipoConta { get; set; }
        public decimal? Valor { get; set; }
        public string? Observacao { get; set; }
        public string? EmpreendimentoCnpj { get; set; }
        public string? EmpreendimentoNome { get; set; }
        public int? PessoaEmpreendimentoId { get; set; }
        public string? NumeroImovel { get; set; }
        public string? FracaoCota { get; set; }
        public string? BlocoCodigo { get; set; }
        public DateTime? LimitePagamentoTransmitido { get; set; }
        public string? ComLimitePagamentoTra { get; set; }
        public string? ComLimitePagamento { get; set; }
        public decimal? ValorJuroDiario { get; set; }
        public decimal? PercentualJuroDiario { get; set; }
        public decimal? PercentualJuroMensal { get; set; }
        public decimal? ValorJuroMensal { get; set; }
        public decimal? PercentualMulta { get; set; }
        public decimal? TaxaJuroMensalProcessamento { get; set; }
        public decimal? TaxaMultaMensalProcessamento { get; set; }

        #region DadosBoleto
        public string? Cedente { get; set; }
        public string? EnderecoCedente { get; set; }
        public string? CnpjCedente { get; set; }
        public string? BancoCodigo { get; set; }
        public string? Agencia { get; set; }
        public string? DigitoAgencia { get; set; }
        public string? Conta { get; set; }
        public string? DigitoConta { get; set; }
        public string? CarteiraPadrao { get; set; }
        public string? VariacaoCarteiraPadrao { get; set; }
        public string? OperacaoConta { get; set; }
        public string? NossoNumero { get; set; }
        public string? SequenciaBoleto { get; set; }
        public string? Convenio { get; set; }
        public string? LinhaDigitavelBoleto { get; set; }
        public string? CodigoBarrasBoleto { get; set; }

        #endregion

    }
}
