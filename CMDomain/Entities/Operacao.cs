namespace CMDomain.Entities
{
    public class Operacao : CMEntityBase
    {
        public virtual int? IdOperacao { get; set; }
        public virtual string? NomeOperacao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
