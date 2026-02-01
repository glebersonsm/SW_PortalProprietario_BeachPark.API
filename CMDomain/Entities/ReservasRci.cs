namespace CMDomain.Entities
{
    public class ReservasRci : CMEntityBase
    {
        public virtual int? IdReservasRci { get; set; }
        public virtual int? IdReservasFront { get; set; }
        public virtual int? IdReservaMigrada { get; set; }
        public virtual string? FlgBulk { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
    }
}
