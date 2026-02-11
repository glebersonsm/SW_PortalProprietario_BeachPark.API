using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Framework.GerenciamentoAcesso;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Framework.GerenciamentoAcesso
{
    public class LogAcessoMap : ClassMap<LogAcesso>
    {
        public LogAcessoMap()
        {
            Id(x => x.Id).GeneratedBy.Native("LogAcesso_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.Guid).Length(200);
            Map(b => b.DataInicio);
            Map(b => b.DataFinal);
            Map(b => b.UrlRequested).Length(4000);
            Map(b => b.ClientIpAddress);
            Map(b => b.RequestBody).CustomType("StringClob").CustomSqlType("Text");
            Map(b => b.Response).CustomType("StringClob").CustomSqlType("Text");
            Map(b => b.StatusResult);

            Schema("portalohana");
            Table("LogAcesso");
        }
    }
}
