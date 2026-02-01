namespace CMDomain.Entities
{
    public class RecbtoPagto : CMEntityBase
    {
        public virtual int? CodDocumento { get; set; }
        public virtual int? NumLancto { get; set; }
        public virtual int? IdUsuarioInclusao { get; set; }
        public virtual int? CodPortForma { get; set; }
        public virtual string? NumChqBordero { get; set; }
        public virtual DateTime? DatacFloat { get; set; }
        public virtual int? CodLancFinanc { get; set; }
        public virtual int? NumLote { get; set; }
        public virtual DateTime? DataBaixa { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

        public override int GetHashCode()
        {
            return CodDocumento.GetHashCode() + NumLancto.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            RecbtoPagto? cc = obj as RecbtoPagto;
            if (cc is null) return false;
            return cc.Equals(this);
        }

    }
}
