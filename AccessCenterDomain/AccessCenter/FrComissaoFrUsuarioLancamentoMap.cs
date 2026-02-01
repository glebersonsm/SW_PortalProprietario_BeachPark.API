using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrComissaoFrUsuarioLancamentoMap : ClassMap<FrComissaoFrUsuarioLancamento>
    {
        public FrComissaoFrUsuarioLancamentoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRCOMISSAOFRUSUARIOLAN_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.FrComissaoFrUsuario);
            Map(b => b.FrFuncao);
            Map(b => b.FrAtendimentoComissao);
            Map(b => b.Observacao);
            Map(b => b.Valor);
            Map(b => b.Parcela);

            Table("FrComissaoFrUsuarioLancamento");
        }
    }
}
