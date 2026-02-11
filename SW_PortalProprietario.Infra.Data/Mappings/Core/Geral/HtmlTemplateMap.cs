using FluentNHibernate.Mapping;
using NHibernate.Type;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class HtmlTemplateMap : ClassMap<HtmlTemplate>
    {
        public HtmlTemplateMap()
        {
            Id(x => x.Id).GeneratedBy.Native("HtmlTemplate_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);

            Map(b => b.DataHoraCriacao);

            Map(b => b.DataHoraAlteracao)
                .Nullable();

            Map(p => p.UsuarioAlteracao)
                .Nullable();

            Map(p => p.Titulo).Length(4000);
            Map(p => p.Header).Length(4000);
            Map(p => p.Content).CustomType("StringClob").CustomSqlType("Text");
            Map(p => p.Consulta).CustomType("StringClob").CustomSqlType("Text");
            Map(p => p.ColunasDeRetorno).Length(4000);
            Map(p => p.ParametrosConsulta).Length(4000);
            Map(p => p.TipoComunicacao).CustomType<EnumType<EnumHtmlTipoComunicacao>>();


            Schema("portalohana");
            Table("HtmlTemplate");
        }
    }
}
