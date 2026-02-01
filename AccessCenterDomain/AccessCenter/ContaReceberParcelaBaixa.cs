namespace AccessCenterDomain.AccessCenter
{
    public class ContaReceberParcelaBaixa : EntityBase
    {
        public virtual int? ContaReceberParcela { get; set; }
        public virtual DateTime? DataBaixa { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual decimal? Multa { get; set; }
        public virtual decimal? Juro { get; set; }
        public virtual decimal? Desconto { get; set; }
        public virtual decimal? TaxaCobranca { get; set; }
        public virtual decimal? ValorAmortizado { get; set; }
        public virtual int? AgrupamConRecParcBai { get; set; }
        public virtual int? TipoContaReceber { get; set; }

        public virtual void Validate()
        {
            if (DataBaixa.HasValue && DataBaixa.GetValueOrDefault() < DateTime.Now.AddYears(-150))
                DataBaixa = DateTime.Now.AddDays(-10).Date;

        }

    }
}
