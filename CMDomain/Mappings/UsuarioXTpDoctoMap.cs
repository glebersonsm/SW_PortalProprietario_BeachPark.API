using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class UsuXTpDoctoMap : ClassMap<UsuarioXTpDocto>
    {
        public UsuXTpDoctoMap()
        {
            CompositeId()
                .KeyProperty(x => x.IdUsuario)
                .KeyProperty(x => x.CodTipDoc);

            Map(b => b.RecPag);
            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("UsuarioXTpDocto");
            Schema("cm");
        }
    }
}
