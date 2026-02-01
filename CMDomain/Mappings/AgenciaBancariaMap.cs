using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class AgenciaBancariaMap : ClassMap<AgenciaBancaria>
    {
        public AgenciaBancariaMap()
        {
            Id(x => x.IdPessoa)
            .GeneratedBy.Assigned();

            Map(p => p.IdBanco);

            Map(b => b.NumAgencia);

            Map(b => b.FlgTipo);

            Map(p => p.FlgAtivo);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("AgenciaBancaria");
        }
    }
}
