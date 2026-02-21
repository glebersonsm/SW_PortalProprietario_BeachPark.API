using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ScItemOcMap : ClassMap<ScItemOc>
    {

        public ScItemOcMap()
        {
            CompositeId()
                .KeyProperty(x => x.IdItemOc)
                .KeyProperty(x => x.NumSolCompra)
                .KeyProperty(x => x.IdItemSoli);


            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("ScItemOc");
            Schema("cm");
        }
    }
}
