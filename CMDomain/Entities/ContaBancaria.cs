namespace CMDomain.Entities
{
    public class ContaBancaria : CMEntityBase
    {
        public virtual int? IdCBancaria { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual string? ContaCorrente { get; set; }
        public virtual int? IdAgencia { get; set; }
        public virtual int? FlgContaPref { get; set; }
        public virtual int? TipoConta { get; set; } = 1;
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual int? FlgInativa { get; set; } = 0;

    }
}
