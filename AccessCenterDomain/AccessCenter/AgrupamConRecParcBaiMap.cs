using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class AgrupamConRecParcBaiMap : ClassMap<AgrupamConRecParcBai>
    {
        public AgrupamConRecParcBaiMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("AGRUPAMCONRECPARCBAI_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.ContaFinanceiraVariacao);
            Map(b => b.ContaFinanceiraSubVariacao);
            Map(b => b.TipoBaixa);
            Map(b => b.MovimentacaoFinanceira);
            Map(b => b.DataBaixa);
            Map(b => b.DataCredito);
            Map(b => b.ValorRecebido);
            Map(b => b.ValorRecebidoDebitoCredito);
            Map(b => b.ValorMulta);
            Map(b => b.ValorJuro);
            Map(b => b.ValorDesconto);
            Map(b => b.ValorTaxaCobranca);
            Map(b => b.ValorAmortizado);
            Map(b => b.ValorAmortizadoDebitoCredito);
            Map(b => b.NumeroDocumento);
            Map(b => b.Observacao);
            Map(b => b.HistoricoMovimentacao);
            Map(b => b.ContaReceberRenegociacao);
            Map(b => b.ContaPagarDestinoSaldo);
            Map(b => b.Contabilizar);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Empresa);
            Map(b => b.Estornado);
            Map(b => b.UsuarioEstorno);
            Map(b => b.DataHoraEstorno);
            Map(b => b.AgrupamentoContaRecParBaiEst);

            Table("AgrupamConRecParcBai");
        }
    }
}
