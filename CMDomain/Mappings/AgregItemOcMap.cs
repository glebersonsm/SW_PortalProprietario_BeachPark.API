using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class AgregItemOcMap : ClassMap<AgregItemOc>
    {
        public AgregItemOcMap()
        {
            Id(x => x.IdAgregItemOc)
            .GeneratedBy.Sequence("SEQAGREGITEMOC");

            Map(p => p.IdItemOc);

            Map(b => b.Aliquota);

            Map(b => b.BaseCalculo);

            Map(p => p.PercBaseCalculo)
                .Nullable();

            Map(b => b.VlrAgregItem)
                .Nullable();

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Schema("cm");
            Table("AgregItemOc");
        }
    }
}
