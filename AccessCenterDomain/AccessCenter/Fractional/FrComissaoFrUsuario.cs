namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrComissaoFrUsuario : EntityBase
    {
        public virtual int MesReferencia { get; set; }
        public virtual int AnoReferencia { get; set; }
        public virtual DateTime? DataInicio { get; set; }
        public virtual DateTime? DataFim { get; set; }
        public virtual int? FrUsuario { get; set; }
        public virtual string Conferido { get; set; }
        public virtual int? ContaPagar { get; set; }
        public virtual decimal Valor { get; set; }

    }
}
