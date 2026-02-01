using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrAtendimentoComissaoMap : ClassMap<FrAtendimentoComissao>
    {
        public FrAtendimentoComissaoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRATENDIMENTOCOMISSAO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Origem);
            Map(b => b.FrAtendimentoVenda);
            Map(b => b.FrAtendimentoFuncao);
            Map(b => b.ContaReceberParcela);
            Map(b => b.FrRegraComissao);
            Map(b => b.ValorBaseCalculo);
            Map(b => b.ValorFundoCobranca);
            Map(b => b.Valor);
            Map(b => b.DataPrevista);
            Map(b => b.PagamentoVinculadoBaixa);
            Map(b => b.Status);
            Map(b => b.DataHoraStatus);
            Map(b => b.FrAtendimentoComissaoOrigem);
            Map(b => b.FrComissaoFrUsuarioLancamento);

            Table("FrAtendimentoComissao");
        }
    }
}
