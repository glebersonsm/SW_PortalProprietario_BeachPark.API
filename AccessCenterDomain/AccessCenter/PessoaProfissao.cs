namespace AccessCenterDomain.AccessCenter
{
    public class PessoaProfissao : EntityBase
    {
        public virtual int Profissao { get; set; }
        public virtual string Principal { get; set; } = "S";
        public virtual int? Pessoa { get; set; }

    }
}
