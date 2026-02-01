namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrAtendimentoVenda : EntityBase
    {
        public virtual int? Filial { get; set; }
        public virtual string IdIntercambiadora { get; set; }
        public virtual string IntegracaoId { get; set; }
        public virtual string Codigo { get; set; }
        public virtual int? FrSala { get; set; } = 823;
        public virtual int? FrAtendimento { get; set; }
        public virtual int? FrPessoa1 { get; set; }
        public virtual int? FrPessoa2 { get; set; }
        public virtual int? FrProduto { get; set; }
        public virtual int? Cota { get; set; }
        public virtual DateTime? DataVenda { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual decimal? ValorBaseComissao { get; set; }
        public virtual decimal? ValorFinanciado { get; set; }
        public virtual decimal? ValorAmortizado { get; set; }
        public virtual decimal? ValorPonto { get; set; }
        public virtual decimal? QuantidaPontos { get; set; }
        public virtual int? TempoUtilizacao { get; set; }
        public virtual string Status { get; set; } = "A";
        public virtual DateTime? DataCancelamento { get; set; }
        public virtual int? UsuarioCancelamento { get; set; }
        public virtual string? BeBack { get; set; }
        public virtual string? ProdutoEntregue { get; set; }
        public virtual DateTime? DataEntregaProduto { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual int? PessoaIndicacao { get; set; }
        public virtual int? SequenciaVendaCota { get; set; }
        public virtual string Contigencia { get; set; } = "N";
        public virtual string Gaveta { get; set; } = "N";
        public virtual int? CotaOriginal { get; set; }
        public virtual int? FrMotivoCancelamento { get; set; }
        public virtual string VirouComissaoV2 { get; set; } = "N";
        public virtual int? FrAtendimentoVendaOrigemRev { get; set; }
        public virtual int? ContaReceberCreditoCan { get; set; }
        public virtual int? ContaReceberMultaCancelamento { get; set; }
        //public virtual int? AgrupamentoContaRecParBaiRev { get; set; }
        public virtual DateTime? DataReversao { get; set; }
        public virtual int? UsuarioReversao { get; set; }
        public virtual decimal? ValorConvertidoFinanciado { get; set; }
        public virtual decimal? ValorConvertidoAmortizado { get; set; }
        public virtual decimal? SaldoReceber { get; set; }
        public virtual DateTime? DataContigencia { get; set; }
        public virtual int? UsuarioContigencia { get; set; }
        public virtual string? TipoMoeda { get; set; }
        public virtual string SemFinanceiro { get; set; } = "N";
        public virtual int? IdPromotorTlmkt { get; set; }
        public virtual int? IdPromotor { get; set; }
        public virtual int? IdLiner { get; set; }
        public virtual int? IdCloser { get; set; }
        public virtual int? IdPep { get; set; }
        public virtual int? IdFtbSugerido { get; set; }
        public virtual int? IdLinerSugerido { get; set; }
        public virtual int? IdCloserSugerido { get; set; }
        public virtual int? IdPepSugerido { get; set; }
        public virtual int? FlgFtb { get; set; }
        public virtual string? PadraoDeCor { get; set; } = "Default";

    }
}
