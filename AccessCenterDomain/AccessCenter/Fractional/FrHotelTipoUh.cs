namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrHotelTipoUh : EntityBase
    {
        public virtual int? Filial { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string IdIntegracao { get; set; }
        public virtual string Status { get; set; } = "A";
        public virtual int? FrHotel { get; set; }
        public virtual int? Capacidade { get; set; }
    }
}
