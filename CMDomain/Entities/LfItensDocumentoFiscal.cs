namespace CMDomain.Entities
{
    public class LfItensDocumento : CMEntityBase
    {
        public virtual int? IdLfItensDocumento { get; set; }
        public virtual int? IdDocumento { get; set; }
        public virtual string? CodStCofins { get; set; } = "50";
        public virtual string? CodStPis { get; set; } = "50";
        public virtual string? CodStb { get; set; } = "41";
        public virtual string? CodUnidade { get; set; } = "UN";
        public virtual string? CodItem { get; set; }
        public virtual string? Cfop { get; set; } = "1933";
        public virtual decimal Quantidade { get; set; } = 1;
        public virtual decimal VlrUnitario { get; set; }
        public virtual decimal VlrContabil { get; set; }
        public virtual decimal VlrOutros { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
