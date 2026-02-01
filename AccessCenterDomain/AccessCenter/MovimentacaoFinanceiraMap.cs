using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class MovimentacaoFinanceiraMap : ClassMap<MovimentacaoFinanceira>
    {
        public MovimentacaoFinanceiraMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("MOVIMENTACAOFIN_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.ContaFinanceiraVariacao);
            Map(b => b.ContaFinanceiraSubVariacao);
            Map(b => b.MovimentacaoFinanceiraOrigem);
            Map(b => b.OperacaoMovFin);
            Map(b => b.Data);
            Map(b => b.Sequencia);
            Map(b => b.Documento);
            Map(b => b.Valor);
            Map(b => b.ValorDebitoCredito);
            Map(b => b.Saldo);
            Map(b => b.SaldoDebitoCredito);
            Map(b => b.Historico);
            Map(b => b.HistoricoContabil);
            Map(b => b.Observacao);
            Map(b => b.TipoDocumento);
            Map(b => b.AgrupamConRecParcBai);
            Map(b => b.ChequeAvulso);
            Map(b => b.ChequePredatado);
            Map(b => b.LancamentoAutomatico);
            Map(b => b.ChequeNumero);
            Map(b => b.ChequeStatus);

            Table("MovimentacaoFinanceira");
        }
    }
}
