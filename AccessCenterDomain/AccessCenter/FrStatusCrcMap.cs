using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrStatusCrcMap : ClassMap<FrStatusCrc>
    {
        public FrStatusCrcMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRSTATUSCRC_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.Status);
            Map(b => b.IntegracaoId);

            Table("FrStatusCrc");
        }
    }
}
