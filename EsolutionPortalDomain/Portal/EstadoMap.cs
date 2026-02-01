using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class EstadoMap : ClassMap<Estado>
    {
        public EstadoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(p => p.Nome);
            Map(b => b.UF);
            Map(b => b.Pais);
            Map(b => b.CodigoIbge);

            Table("Estado");
        }
    }
}
