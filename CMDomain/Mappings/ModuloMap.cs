using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ModuloMap : ClassMap<Modulo>
    {
        public ModuloMap()
        {
            Id(x => x.IdModulo).GeneratedBy.Assigned();

            Map(p => p.NomeModulo);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("Modulo");
        }
    }
}
