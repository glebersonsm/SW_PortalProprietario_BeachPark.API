using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class SwLogsMap : ClassMap<SwLogs>
    {
        public SwLogsMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("SWLOGSSEQ_");

            Map(p => p.Usuario);
            Map(b => b.Tipo);
            Map(b => b.DataHoraCriacao);
            Map(p => p.BodyRequisicao);
            Map(p => p.Mensagem);

            Table("SwLogs");
        }
    }
}
