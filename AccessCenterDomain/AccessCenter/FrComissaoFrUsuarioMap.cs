using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrComissaoFrUsuarioMap : ClassMap<FrComissaoFrUsuario>
    {
        public FrComissaoFrUsuarioMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRCOMISSAOFRUSUARIO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.MesReferencia);
            Map(b => b.AnoReferencia);
            Map(b => b.DataInicio);
            Map(b => b.DataFim);
            Map(b => b.FrUsuario);
            Map(b => b.Conferido);
            Map(b => b.ContaPagar);
            Map(b => b.Valor);

            Table("FrComissaoFrUsuario");
        }
    }
}
