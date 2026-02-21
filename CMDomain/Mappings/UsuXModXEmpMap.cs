using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class UsuXModXEmpMap : ClassMap<UsuXModXEmp>
    {
        public UsuXModXEmpMap()
        {
            CompositeId()
                .KeyProperty(x => x.IdEspAcesso)
                .KeyProperty(x => x.IdEmpresa)
                .KeyProperty(x => x.IdModulo);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("UsuXModXEmp");
            Schema("cm");
        }
    }
}
