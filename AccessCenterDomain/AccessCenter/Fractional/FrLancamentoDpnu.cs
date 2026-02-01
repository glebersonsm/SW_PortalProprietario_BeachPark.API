namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrLancamentoDpnu : EntityBase
    {
        public virtual string Estornado { get; set; } = "N";
        public virtual string TipoLancamentoDpnu { get; set; } = "N";
        public virtual DateTime? DatainicioApropriacao { get; set; }
        public virtual DateTime? DataFimApropriacao { get; set; }
        public virtual DateTime? DataProcessamento { get; set; }
        public virtual int? FrDpnu { get; set; }
        public virtual string Status { get; set; } = "P";

    }
}
