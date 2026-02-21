using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class FuncaoMap : ClassMap<Funcao>
    {
        public FuncaoMap()
        {
            Id(x => x.IdFuncao).GeneratedBy.Assigned();

            Map(p => p.NomeFuncao);
            Map(p => p.IdModulo);
            Map(p => p.IdFuncaoPai);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);


            Schema("cm");
            Table("Funcao");
        }
    }
}
