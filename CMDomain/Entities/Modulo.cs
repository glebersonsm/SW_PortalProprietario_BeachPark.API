namespace CMDomain.Entities
{
    public class Modulo : CMEntityBase
    {
        public virtual int? IdModulo { get; set; }
        public virtual string? NomeModulo { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
