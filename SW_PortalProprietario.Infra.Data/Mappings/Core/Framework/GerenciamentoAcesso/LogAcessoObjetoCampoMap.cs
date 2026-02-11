using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Framework.GerenciamentoAcesso;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Framework.GerenciamentoAcesso
{
    public class LogAcessoObjetoCampoMap : ClassMap<LogAcessoObjetoCampo>
    {
        public LogAcessoObjetoCampoMap()
        {
            Id(x => x.Id).GeneratedBy.Native("LogAcessoObjetoCampo_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.TipoCampo).Length(500);
            Map(b => b.NomeCampo).Length(200);
            Map(b => b.ValorAntes).CustomType("StringClob").CustomSqlType("Text");
            Map(b => b.ValorApos).CustomType("StringClob").CustomSqlType("Text");

            Schema("portalohana");
            Table("LogAcessoObjetoCampo");
        }
    }
}
