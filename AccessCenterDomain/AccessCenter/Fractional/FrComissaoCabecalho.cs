namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrComissaoCabecalho : EntityBase
    {
        public virtual string NomeRegra { get; set; }
        public virtual int? FrAtendimento { get; set; }
        public virtual int? FrAtendimentoVenda { get; set; }
        public virtual int? FrAtendimentoFuncao { get; set; }
        public virtual string Finalizado { get; set; } = "N";
        public virtual string Tipo { get; set; } = "B";
        public virtual int? QuantidadeParcelas { get; set; }
        public virtual int? QuantidadeMaximaParcelas { get; set; }
        public virtual decimal? ValorTotalComissao { get; set; }
        public virtual decimal? ValorFimIntegralizacao { get; set; }
        public virtual decimal? ValorInicioIntegralizacao { get; set; }
        public virtual int? DiasRemoverDataDinheiro { get; set; }
        public virtual int? DiasRemoverDataCredito { get; set; }
        public virtual int? DiasRemoverDataDebito { get; set; }
        public virtual string TipoDataDinheiro { get; set; } = "B";
        public virtual string TipoDataCredito { get; set; } = "B";
        public virtual string TipoDataDebito { get; set; } = "B";

    }
}
