using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class ComunicacaoTokenEnviadaMap : ClassMap<ComunicacaoTokenEnviada>
    {
        public ComunicacaoTokenEnviadaMap()
        {
            Id(x => x.Id).GeneratedBy.Native("ComunicacaoTokenEnviada_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao).Nullable();
            Map(b => b.DataHoraCriacao).Nullable();
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();

            References(x => x.Usuario, "Usuario").Nullable();
            Map(x => x.Login).Length(200);
            Map(x => x.Canal).Length(20).Not.Nullable();
            Map(x => x.Destinatario).Length(500);
            Map(x => x.TextoEnviado).CustomType("StringClob").CustomSqlType("Text");
            Map(x => x.DataHoraEnvio).Not.Nullable();
            Map(x => x.TwoFactorId).Nullable();
            Map(x => x.EmailId).Nullable();

            Table("ComunicacaoTokenEnviada");
        }
    }
}
