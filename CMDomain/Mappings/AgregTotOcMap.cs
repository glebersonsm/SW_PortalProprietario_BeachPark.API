using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class AgregTotOcMap : ClassMap<AgregTotOc>
    {
        public AgregTotOcMap()
        {
            Id(x => x.IdAgregTotOc)
            .GeneratedBy.Sequence("SEQAGREGTOTOC");

            Map(p => p.CodTipoCustAgreg);
            Map(p => p.NumOc);

            Map(b => b.Aliquota);

            Map(b => b.BaseCalculo);

            Map(p => p.PercBaseCalculo)
                .Nullable();

            Map(b => b.VlrAgregTot)
                .Nullable();

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Schema("cm");
            Table("AgregTotOc");
        }
    }
}
