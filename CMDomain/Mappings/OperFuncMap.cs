using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class OperFuncMap : ClassMap<OperFunc>
    {
        public OperFuncMap()
        {
            Id(x => x.IdOperFunc).GeneratedBy.Assigned();
            Map(p => p.IdModulo);
            Map(p => p.IdOperacao);
            Map(p => p.IdFuncao);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("OperFunc");
            Schema("cm");
        }
    }
}
