namespace CMDomain.Entities
{
    public class ReservasTs : CMEntityBase
    {
        public virtual int? IdReservasFront { get; set; }
        public virtual int? IdDisponibilidade { get; set; }
        public virtual DateTime? DataChegada { get; set; }
        public virtual DateTime? DataPartida { get; set; }
        public virtual DateTime? DataConfirmacao { get; set; }
        public virtual int? IdMotivoTs { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
    }
}
