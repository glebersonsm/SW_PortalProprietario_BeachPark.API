namespace CMDomain.Entities
{
    public class AgenciaBancaria : CMEntityBase
    {
        public virtual int? IdPessoa { get; set; }
        public virtual int? IdBanco { get; set; }
        public virtual string? NumAgencia { get; set; }
        public virtual string? FlgTipo { get; set; }
        public virtual string? FlgAtivo { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

    }
}
