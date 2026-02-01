namespace AccessCenterDomain.AccessCenter
{
    public class ContaReceberParcelaOrigem : EntityBase
    {
        public virtual int? ContaReceberParcela { get; set; }
        public virtual int? ContaReceberParcelaDestino { get; set; }
        public virtual string Tipo { get; set; } = "O";
        public virtual decimal? Valor { get; set; }
        public virtual int? ContaReceberParcelaAltVal { get; set; }

    }
}
