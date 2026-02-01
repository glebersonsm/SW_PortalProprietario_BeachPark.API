namespace CMDomain.Entities
{
    public class FracionamentoTs : CMEntityBase
    {
        public virtual int? IdFracionamentoTs { get; set; }
        public virtual int? IdReservasFront1 { get; set; }
        public virtual int? IdReservasFront2 { get; set; }
        public virtual int? IdVendaXContrato { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }


    }
}
