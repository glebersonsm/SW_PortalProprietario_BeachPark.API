using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class PaisMap : ClassMap<Pais>
    {
        public PaisMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("PAIS_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Nome);
            Map(p => p.NomePesquisa);
            Map(p => p.DDI);
            Map(p => p.CodigoPais);

            Table("Pais");
        }
    }
}
