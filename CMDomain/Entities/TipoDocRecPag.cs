namespace CMDomain.Entities
{
    public class TipoDocRecPag : CMEntityBase
    {
        public virtual int? CodTipDoc { get; set; }
        public virtual string? RecPag { get; set; }
        public virtual string? Descricao { get; set; }
        public virtual string? FlgServico { get; set; }
        public virtual string? FlgAtivo { get; set; }
        public virtual string? FlgDocFiscal { get; set; }
        public virtual string? CodTipDocReceb { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
