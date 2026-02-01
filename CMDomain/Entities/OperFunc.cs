namespace CMDomain.Entities
{
    public class OperFunc : CMEntityBase
    {
        public virtual int? IdOperFunc { get; set; }
        public virtual int? IdModulo { get; set; }
        public virtual int? IdOperacao { get; set; }
        public virtual int? IdFuncao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
