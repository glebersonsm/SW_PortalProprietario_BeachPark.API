namespace CMDomain.Entities
{
    public class FornServ : CMEntityBase
    {
        public virtual int? IdPessoa { get; set; }
        public virtual int? FlgAss { get; set; } = 0;
        public virtual string? CodCorresp { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
    }
}
