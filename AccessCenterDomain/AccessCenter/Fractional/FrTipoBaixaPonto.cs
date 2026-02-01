namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrTipoBaixaPonto : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; } = 1;
        public virtual int? Empresa { get; set; } = 1;
        public virtual int? Filial { get; set; } = 1;
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string Status { get; set; }
        public virtual string Reserva { get; set; }
        public virtual string DebitoCredito { get; set; }
        public virtual string LancamentoFuturo { get; set; }
        public virtual int? LimiteUtilizacao { get; set; }
        public virtual int? LancamentoManual { get; set; }
        public virtual string Dpnu { get; set; }
        public virtual string ComporTotalPontosContrato { get; set; } = "N";
    }
}
