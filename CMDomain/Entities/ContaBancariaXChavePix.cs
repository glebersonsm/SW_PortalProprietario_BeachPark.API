namespace CMDomain.Entities
{
    public class ContaBancariaXChavePix : CMEntityBase
    {
        public virtual int? IdContaXChave { get; set; }
        public virtual int? IdCBancaria { get; set; }
        public virtual string? ChavePix { get; set; }
        public virtual int? FlgChavePref { get; set; }
        public virtual int? IdTipoChave { get; set; }

    }
}
