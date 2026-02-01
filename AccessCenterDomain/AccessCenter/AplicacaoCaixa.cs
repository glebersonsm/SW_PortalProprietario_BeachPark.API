namespace AccessCenterDomain.AccessCenter
{
    public class AplicacaoCaixa : EntityBase
    {
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual int? AplicacaoCaixaGrupo { get; set; }
        public virtual string StatusLancamento { get; set; } = "A";
        public virtual string StatusConsulta { get; set; } = "A";
        public virtual string ContaReceber { get; set; } = "S";
        public virtual string ContaPagar { get; set; } = "S";
        public virtual string MovimentacaoFinanceiraManual { get; set; } = "S";


    }
}
