using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class UsuXAlmoxMap : ClassMap<UsuXAlmox>
    {
        public UsuXAlmoxMap()
        {
            CompositeId()
                .KeyProperty(x => x.CodAlmoxarifado)
                .KeyProperty(x => x.IdPessoa)
                .KeyProperty(x => x.IdUsuario);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("UsuXAlmox");
            Schema("cm");
        }
    }
}
