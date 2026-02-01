namespace AccessCenterDomain.AccessCenter
{
    public class ContaReceberBoleto : EntityBase
    {
        public virtual int? Cliente { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual int? Filial { get; set; }
        public virtual DateTime? Emissao { get; set; }
        public virtual DateTime? Vencimento { get; set; }
        public virtual DateTime? VencimentoOriginal { get; set; }
        public virtual DateTime? LimitePagamentoTransmitido { get; set; }
        public virtual DateTime? LimitePagamento { get; set; }
        public virtual string? ComLimitePagamentoTra { get; set; }
        public virtual string? ComLimitePagamento { get; set; }
        public virtual string? CodigoBarras { get; set; }
        public virtual string? LinhaDigitavel { get; set; }
        public virtual string? NossoNumero { get; set; }
        public virtual int? Sequencia { get; set; }
        public virtual decimal? ValorBoleto { get; set; }
        public virtual decimal? ValorBoletoOriginal { get; set; }
        public virtual decimal? PercentualJuroDiario { get; set; }
        public virtual decimal? ValorJuroDiario { get; set; }
        public virtual decimal? PercentualJuroMensal { get; set; }
        public virtual decimal? ValorJuroMensal { get; set; }
        public virtual decimal? MultaBoleto { get; set; }
        public virtual decimal? PercentualMulta { get; set; }
        public virtual decimal? ValorBaixa { get; set; }
        public virtual string? BoletoImpresso { get; set; } = "S";
        public virtual int? QuantidadeImpressoes { get; set; } = 1;
        public virtual string? Status { get; set; }
        public virtual int? Banco { get; set; }
        public virtual int? ContaFinVariConCob { get; set; }
        public virtual string? Parcela { get; set; }
        public virtual string? ManterNossoNumero { get; set; } = "N";
        public virtual string? Descricao { get; set; }
        public virtual int? QuantidadeParcelasVinculadas { get; set; } = 1;
        public virtual DateTime? DataHoraBaixa { get; set; }
        public virtual int? UsuarioBaixa { get; set; }
        public virtual DateTime? DataHoraCancelamento { get; set; }
        public virtual int? UsuarioCancelamento { get; set; }
    }
}
