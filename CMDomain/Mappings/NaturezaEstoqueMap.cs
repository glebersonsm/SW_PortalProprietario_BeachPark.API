using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class NaturezaEstoqueMap : ClassMap<NaturezaEstoque>
    {
        public NaturezaEstoqueMap()
        {
            Id(x => x.IdNaturezaEstoque).GeneratedBy.Assigned();

            Map(p => p.CodNatureza);
            Map(p => p.DescNatureza);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("NaturezaEstoque");
            Schema("cm");
        }
    }
}
