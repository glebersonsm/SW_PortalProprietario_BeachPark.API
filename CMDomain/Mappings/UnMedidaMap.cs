using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class UnMedidaMap : ClassMap<UnMedida>
    {
        public UnMedidaMap()
        {
            Id(x => x.CodMedida).GeneratedBy.Assigned();

            Map(p => p.DescMedida);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("UnMedida");
            Schema("cm");
        }
    }
}
