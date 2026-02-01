using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ProcessoMap : ClassMap<Processo>
    {
        public ProcessoMap()
        {
            Id(p => p.CodProcesso)
                .GeneratedBy.Sequence("SEQPROCESSO");
            Map(p => p.Status);
            Map(p => p.IdProcesso);
            Map(p => p.IdComprador);
            Map(p => p.CodGrupoProd);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("Processo");
        }
    }
}
