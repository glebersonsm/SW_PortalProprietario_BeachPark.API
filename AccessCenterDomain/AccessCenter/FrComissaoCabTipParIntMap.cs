using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrComissaoCabTipParIntMap : ClassMap<FrComissaoCabTipParInt>
    {
        public FrComissaoCabTipParIntMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRCOMISSAOCABTIPPARINT_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.FrComissaoCabecalho);
            Map(b => b.TipoParcela);
            Map(b => b.ComissaoCabecalho);

            Table("FrComissaoCabTipParInt");
        }
    }
}
