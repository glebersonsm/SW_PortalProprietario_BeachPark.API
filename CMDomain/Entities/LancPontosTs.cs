namespace CMDomain.Entities
{
    public class LancPontosTs : CMEntityBase
    {
        public virtual int? IdLancPontosTs { get; set; }
        public virtual int? IdVendaXContrato { get; set; }
        public virtual decimal? NumeroPontos { get; set; }
        public virtual string? DebitoCredito { get; set; }
        public virtual int? IdReservasFront { get; set; }
        public virtual int? IdUsuario { get; set; }
        public virtual DateTime? DataLancamento { get; set; }
        public virtual int? IdHotel { get; set; }
        public virtual int? IdUsuarioLogado { get; set; }
        public virtual string? FlgMigrado { get; set; } = "N";
        public virtual int? IdTipoLancPontoTs { get; set; }
        public virtual DateTime? ValidadeCredito { get; set; }
        public virtual string? FlgVlrManual { get; set; } = "N";
        public virtual string? FlgAssociada { get; set; } = "N";
        public virtual int? IdUsuarioReserva { get; set; }
        public virtual int? IdContrXPontoCobrado { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
