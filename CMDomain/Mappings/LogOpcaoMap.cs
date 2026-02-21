using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class LogOpcaoMap : ClassMap<LogOpcao>
    {
        public LogOpcaoMap()
        {
            Id(x => x.IdLogOpcao).GeneratedBy.Assigned();

            Map(p => p.IdUsuario);
            Map(p => p.IdPessoa);
            Map(p => p.IdModulo);
            Map(p => p.NomeOpcao);
            Map(p => p.DataLog);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("LogOpcao");
            Schema("cm");
        }
    }
}
