using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class BancoMap : ClassMap<Banco>
    {
        public BancoMap()
        {
            Id(x => x.IdPessoa)
            .GeneratedBy.Assigned();

            Map(p => p.NumBanco);

            Map(b => b.MascaraCC);

            Map(b => b.MascaraAgencia);
            Map(p => p.QdvAgencia);
            Map(p => p.QdvCC);

            Map(p => p.FlgValidaCC);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("Banco");
        }
    }
}
