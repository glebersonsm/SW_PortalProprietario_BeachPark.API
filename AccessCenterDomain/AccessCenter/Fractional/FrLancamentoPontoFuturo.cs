namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrLancamentoPontoFuturo : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; } = 1;
        public virtual int? Empresa { get; set; } = 1;
        public virtual int? Filial { get; set; } = 1;
        public virtual DateTime? DataSolicitacao { get; set; }
        public virtual int? FrAtendimentoVenda { get; set; }
        public virtual int? FrTipoBaixaPonto { get; set; }
        public virtual int? FrLancamentoDpnu { get; set; }
        public virtual decimal? TotalPontos { get; set; }
        public virtual decimal? TotalPontoUtilizado { get; set; }
        public virtual int? QuantidadeDiasTotal { get; set; }
        public virtual int? QuantidadeDiasUtilizado { get; set; }
        public virtual DateTime? DataLimiteUtilizacao { get; set; }
        public virtual string Estornado { get; set; } = "N";
    }
}
