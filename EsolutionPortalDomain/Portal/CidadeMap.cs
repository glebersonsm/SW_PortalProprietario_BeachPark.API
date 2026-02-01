using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class CidadeMap : ClassMap<Cidade>
    {
        public CidadeMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(p => p.Nome);
            Map(b => b.UF);
            Map(b => b.Pais);
            Map(b => b.CodigoIbge);
            Map(b => b.Estado);
            Map(b => b.CodigoMunicipioNFSe);

            Table("Cidade");
        }
    }
}
