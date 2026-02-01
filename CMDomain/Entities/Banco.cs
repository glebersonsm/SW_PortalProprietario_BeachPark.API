namespace CMDomain.Entities
{
    public class Banco : CMEntityBase
    {
        public virtual int? IdPessoa { get; set; }
        public virtual string? NumBanco { get; set; }
        public virtual string? MascaraCC { get; set; }
        public virtual string? MascaraAgencia { get; set; }
        public virtual string? FlgValidaCC { get; set; }
        public virtual int? QdvAgencia { get; set; }
        public virtual int? QdvCC { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

    }
}
