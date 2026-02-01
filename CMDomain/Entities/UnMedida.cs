namespace CMDomain.Entities
{
    public class UnMedida : CMEntityBase
    {
        public virtual string? CodMedida { get; set; }
        public virtual string? DescMedida { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
    }
}
