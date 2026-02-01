using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class PaisMap : ClassMap<Pais>
    {
        public PaisMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(p => p.Nome);
            Map(b => b.DDI);
            Map(b => b.CodigoPais);
            Map(b => b.CodigoFNRH);

            Table("Pais");
        }
    }
}
