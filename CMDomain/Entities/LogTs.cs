namespace CMDomain.Entities
{
    public class LogTs : CMEntityBase
    {
        public virtual int? IdUsuario { get; set; }
        public virtual int? IdTipoLogTs { get; set; }
        public virtual int? IdLogTs { get; set; }
        public virtual DateTime? DataSistema { get; set; }
        public virtual DateTime? DataHora { get; set; }
        public virtual int? Chave { get; set; }
        public virtual int? IdUsuarioAut { get; set; }
        public virtual int? IdLancPagRecorrenteTs { get; set; }
        public virtual int? IdLancamentoTs { get; set; }
        public virtual string? Status { get; set; }
        public virtual int? IdVendaXContrato { get; set; }
        public virtual int? IdCliente { get; set; }
        public virtual int? IdInfoCartao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
