namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrFuncao : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? CargoPedido { get; set; }
        public virtual string GeraContaPagar { get; set; }
        public virtual string FTB { get; set; }
        public virtual string LancamentoAutomatico { get; set; } = "N";
        public virtual string Fase { get; set; }
        public virtual string FaseStatus { get; set; }
        public virtual string ExigeEquipe { get; set; }
        public virtual string PermitirSomenteUmNaEquipe { get; set; }
        public virtual string PermitirVinculoMaisDeUmaEquipe { get; set; }
        public virtual string ExibeContrato { get; set; }
        public virtual string ExibeCentralAtendimento { get; set; }

        //Em qual fase essa função aparece no relatorio brinde concedido
        public virtual string FaseRelatorioBrindeConcedido { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string SwVinculos { get; set; }

    }
}
