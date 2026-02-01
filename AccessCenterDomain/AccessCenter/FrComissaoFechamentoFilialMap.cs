using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class FrComissaoFechamentoFilialMap : ClassMap<FrComissaoFechamentoFilial>
    {
        public FrComissaoFechamentoFilialMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRCOMISSAOFECHAMENTOFILIAL_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.FrComissaoFechamento);
            Map(b => b.Filial);

            Table("FrComissaoFechamentoFilial");
        }
    }
}
