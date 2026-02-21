using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class CentResponMap : ClassMap<CentRespon>
    {
        public CentResponMap()
        {
            CompositeId()
                .KeyProperty(p => p.CodCentroRespon)
                .KeyProperty(p => p.IdPessoa);

            Map(p => p.Nome);
            Map(p => p.AnaliticoSintet);
            Map(p => p.Ativo);

            Schema("cm");
            Table("CentRespon");
        }
    }
}
