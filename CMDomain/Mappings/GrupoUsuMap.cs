using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class GrupoUsuMap : ClassMap<GrupoUsu>
    {
        public GrupoUsuMap()
        {
            CompositeId()
                .KeyProperty(p => p.IdGrupo)
                .KeyProperty(p => p.IdUsuario);

            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Schema("cm");
            Table("GrupoUsu");
        }
    }
}
