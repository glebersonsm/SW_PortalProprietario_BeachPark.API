using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class FrComissaoLancamentoMap : ClassMap<FrComissaoLancamento>
    {
        public FrComissaoLancamentoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRCOMISSAOLANCAMENTO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.FrAtendimento);
            Map(b => b.FrAtendimentoVenda);
            Map(b => b.FrAtendimentoFuncao);
            Map(b => b.FrComissaoCabecalho);
            Map(b => b.FrComissaoFechamento);
            Map(b => b.Previsao);
            Map(b => b.Bloqueada);
            Map(b => b.Cancelada);
            Map(b => b.LancamentoManual);
            Map(b => b.Data);
            Map(b => b.Valor);
            Map(b => b.MemoriaCalculo);
            Map(b => b.MemoriaFechamento);
            Map(b => b.ContaReceberParcela);

            Table("FrComissaoLancamento");
        }
    }
}
