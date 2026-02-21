using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class OperacaoMap : ClassMap<Operacao>
    {
        public OperacaoMap()
        {
            Id(x => x.IdOperacao).GeneratedBy.Assigned();
            Map(p => p.NomeOperacao);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("Operacao");
            Schema("cm");
        }
    }
}
