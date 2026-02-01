using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class OperacaoFinanceiraMap : ClassMap<OperacaoFinanceira>
    {
        public OperacaoFinanceiraMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("OPERACAOFINANCEIRA_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.IdReferencia);
            Map(b => b.DataHoraAlteracaoReferencia);
            Map(b => b.Empresa);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.Tipo);
            Map(b => b.Status);
            Map(b => b.ContabilizarLancamento);
            Map(b => b.ContabilizarAlteracao);
            Map(b => b.ExigeLancamentoDespesa);
            Map(b => b.PermitirLancamentoManual);
            Map(b => b.ContabilizacaoRegra);
            Map(b => b.PermitirDocumentoMesmoNumCli);

            Table("OperacaoFinanceira");
        }
    }
}
