using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class CestMap : ClassMap<Cest>
    {
        public CestMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CEST_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);

            Table("Cest");
        }
    }
}
