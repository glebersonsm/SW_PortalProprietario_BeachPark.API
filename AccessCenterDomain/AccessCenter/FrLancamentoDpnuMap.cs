using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrLancamentoDpnuMap : ClassMap<FrLancamentoDpnu>
    {
        public FrLancamentoDpnuMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRLANCAMENTODPNU_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Estornado);
            Map(b => b.TipoLancamentoDpnu);
            Map(b => b.DataFimApropriacao);
            Map(b => b.DataProcessamento);
            Map(b => b.FrDpnu);
            Map(b => b.Status);

            Table("FrLancamentoDpnu");
        }
    }
}
