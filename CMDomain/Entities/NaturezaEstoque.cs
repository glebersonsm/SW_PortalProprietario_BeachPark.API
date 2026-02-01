namespace CMDomain.Entities
{
    public class NaturezaEstoque : CMEntityBase
    {
        public virtual int? IdNaturezaEstoque { get; set; }
        public virtual string? CodNatureza { get; set; }
        public virtual string? DescNatureza { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
    }
}
