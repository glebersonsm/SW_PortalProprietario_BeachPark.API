using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class TipoEnderecoMap : ClassMap<TipoEndereco>
    {
        public TipoEnderecoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(p => p.Nome);
            Map(b => b.Codigo);

            Table("TipoEndereco");
        }
    }
}
