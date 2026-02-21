using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class UsuXGrupProdMap : ClassMap<UsuXGrupProd>
    {
        public UsuXGrupProdMap()
        {
            CompositeId()
                .KeyProperty(x => x.CodGrupoProd)
                .KeyProperty(x => x.IdPessoa)
                .KeyProperty(x => x.IdUsuario);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("UsuXGrupProd");
            Schema("cm");
        }
    }
}
