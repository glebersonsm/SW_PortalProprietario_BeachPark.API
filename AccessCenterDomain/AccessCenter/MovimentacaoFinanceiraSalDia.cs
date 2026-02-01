namespace AccessCenterDomain.AccessCenter
{
    public class MovimentacaoFinanceiraSalDia : EntityBase
    {
        public virtual int? Empresa { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual DateTime? Data { get; set; }
        public virtual decimal TotalDebito { get; set; }
        public virtual decimal TotalCredito { get; set; }
        public virtual decimal TotalSaldo { get; set; }
        public virtual int? ContaFinanceiraVariacao { get; set; }
        public virtual int? ContaFinanceiraSubVariacao { get; set; }

    }
}
