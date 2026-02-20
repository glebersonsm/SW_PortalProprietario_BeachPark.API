using FluentNHibernate.Mapping;
using NHibernate.Type;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class DocumentTemplateMap : ClassMap<DocumentTemplate>
    {
        public DocumentTemplateMap()
        {
            Schema("portalohana");
            Table("DocumentTemplate");

            Id(x => x.Id).GeneratedBy.Native("DocumentTemplate_");
            Map(x => x.ObjectGuid).Length(100);
            Map(x => x.UsuarioCriacao);
            Map(x => x.DataHoraCriacao);
            Map(x => x.UsuarioAlteracao).Nullable();
            Map(x => x.DataHoraAlteracao).Nullable();

            Map(x => x.TemplateType).CustomType<EnumType<EnumDocumentTemplateType>>();
            Map(x => x.Name).Length(255).Not.Nullable();
            Map(x => x.Version).Not.Nullable();
            Map(x => x.ContentHtml).CustomSqlType("Text").Not.Nullable();
            Map(x => x.Active).Not.Nullable();
        }
    }
}

