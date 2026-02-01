using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class FornServMap : ClassMap<FornServ>
    {
        public FornServMap()
        {
            Id(x => x.IdPessoa);

            Map(p => p.FlgAss);
            Map(p => p.CodCorresp);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("FornServ");
        }
    }
}
