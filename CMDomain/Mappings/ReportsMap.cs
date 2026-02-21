using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ReportsMap : ClassMap<Reports>
    {
        public ReportsMap()
        {
            CompositeId().KeyProperty(p => p.IdReports)
                .KeyProperty(p => p.OrigemCM);

            Map(p => p.IdModulo);
            Map(p => p.IdGrupoRelatorio);
            Map(p => p.Name);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("Reports");
            Schema("cm");
        }
    }
}
