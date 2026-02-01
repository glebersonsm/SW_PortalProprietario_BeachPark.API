namespace CMDomain.Entities
{
    public class FornXDesemb : CMEntityBase
    {
        public virtual int? IdFornXDesemb { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual string? CodTipRecDes { get; set; }
        public virtual string? RecPag { get; set; } = "P";
        public virtual int? IdEmpresaProp { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
