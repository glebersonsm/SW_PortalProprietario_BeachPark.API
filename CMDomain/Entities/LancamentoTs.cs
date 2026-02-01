namespace CMDomain.Entities
{
    public class LancamentoTs : CMEntityBase
    {
        public virtual int? IdLancamentoTs { get; set; }
        public virtual int? IdLancPontosTs { get; set; }
        public virtual int? IdTipoDebCred { get; set; }
        public virtual int? IdVendaTs { get; set; }
        public virtual int? IdHotel { get; set; }
        public virtual decimal? VlrLancamento { get; set; }
        public virtual decimal? VlrAPagar { get; set; }
        public virtual int? IdTipoLancamento { get; set; }
        public virtual DateTime? DataLancamento { get; set; }
        public virtual DateTime? DataPagamento { get; set; }
        public virtual string? Documento { get; set; }
        public virtual int? IdUsuario { get; set; }
        public virtual DateTime? ValidadeCredito { get; set; }
        public virtual string? FlgTaxaAdm { get; set; } = " ";
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
