using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class NcmMap : ClassMap<Ncm>
    {
        public NcmMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("NCM_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            References(b => b.Cest, "Cest");

            Table("Ncm");
        }
    }
}
