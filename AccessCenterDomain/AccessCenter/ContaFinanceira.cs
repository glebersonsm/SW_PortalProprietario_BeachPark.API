namespace AccessCenterDomain.AccessCenter
{
    public class ContaFinanceira : EntityBase
    {
        public virtual int? Filial { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual string ContaFinanceiraTipo { get; set; }
        public virtual string Status { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string ContaNumero { get; set; }
        public virtual string ContaDigito { get; set; }
        public virtual string AgenciaNumero { get; set; }
        public virtual string AgenciaDigito { get; set; }
        public virtual int? Banco { get; set; }
        public virtual string Cedente { get; set; }
        public virtual string EnderecoCedente { get; set; }
        public virtual Int64? CNPJCedente { get; set; }

    }

}
