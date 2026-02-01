using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class TipoBaixaMap : ClassMap<TipoBaixa>
    {
        public TipoBaixaMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("TIPOBAIXA_SEQUENCE");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.Empresa);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.LancaMovimentacaoFinanceira);
            Map(b => b.TipoBaixaAplicacao);
            Map(b => b.OperacaoMovimentacaoFinanceira);
            Map(b => b.Contabilizar);
            Map(b => b.ConciliacaoRecebiveis);
            Map(b => b.LocalContabilizacao);
            Map(b => b.ContabilizacaoRegra);
            Map(b => b.Renegociar);
            Map(b => b.Transferencia);
            Map(b => b.TipoTransferencia);
            Map(b => b.PermiteLancamentoManual);
            Map(b => b.DepositoNaoIdentificado);

            Table("TipoBaixa");
        }
    }
}
