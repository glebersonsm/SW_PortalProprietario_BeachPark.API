namespace AccessCenterDomain.AccessCenter
{
    public class OperacaoFinanceira : EntityBase
    {

        public virtual int? IdReferencia { get; set; }
        public virtual DateTime? DataHoraAlteracaoReferencia { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string Tipo { get; set; } = "R";
        public virtual string Status { get; set; }
        public virtual string ContabilizarLancamento { get; set; } = "N";
        public virtual string ContabilizarAlteracao { get; set; } = "N";
        public virtual string ExigeLancamentoDespesa { get; set; } = "N";
        public virtual string PermitirLancamentoManual { get; set; } = "N";
        public virtual int? ContabilizacaoRegra { get; set; }
        public virtual string PermitirDocumentoMesmoNumCli { get; set; }

    }
}
