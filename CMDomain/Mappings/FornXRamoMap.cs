using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class FornXRamoMap : ClassMap<FornXRamo>
    {
        public FornXRamoMap()
        {
            CompositeId()
                .KeyProperty(x => x.IdRamoFornecedor)
                .KeyProperty(x => x.IdPessoa);

            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Schema("cm");
            Table("FornXRamo");
        }
    }
}
