using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class UsCCustoMap : ClassMap<UsCCusto>
    {
        public UsCCustoMap()
        {
            CompositeId()
                .KeyProperty(x => x.CodCentroCusto)
                .KeyProperty(x => x.IdEmpresa)
                .KeyProperty(x => x.IdPessoa)
                .KeyProperty(x => x.IdUsuario);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("UsCCusto");
            Schema("cm");
        }
    }
}
