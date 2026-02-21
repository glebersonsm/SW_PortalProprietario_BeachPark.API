using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ConverMap : ClassMap<Conver>
    {
        public ConverMap()
        {
            CompositeId()
                .KeyProperty(p => p.CodProduto)
                .KeyProperty(p => p.CodMedida);

            Map(p => p.Fator);
            Map(p => p.FlgAtivo);
            Map(p => p.TrgUserInclusao);
            Map(p => p.TrgDtInclusao);

            Schema("cm");
            Table("Conver");
        }
    }
}
