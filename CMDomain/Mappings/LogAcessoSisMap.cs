using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class LogAcessoSisMap : ClassMap<LogAcessoSis>
    {
        public LogAcessoSisMap()
        {
            Id(x => x.IdLogAcessoSis).GeneratedBy.Sequence("SEQLOGACESSOSIS");

            Map(p => p.IdUsuario);
            Map(p => p.IdModulo);
            Map(p => p.FlgOperacao);
            Map(p => p.Versao);

            Table("LogAcessoSis");
            Schema("cm");
        }
    }
}
