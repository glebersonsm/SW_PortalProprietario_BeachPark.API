using FluentNHibernate.Mapping;
using NHibernate.Type;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class AutomaticCommunicationConfigMap : ClassMap<AutomaticCommunicationConfig>
    {
        public AutomaticCommunicationConfigMap()
        {
            Id(x => x.Id).GeneratedBy.Native("AutomaticCommunicationConfig_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();

            Map(b => b.CommunicationType).CustomType<EnumType<EnumDocumentTemplateType>>();
            Map(b => b.ProjetoType).CustomType<EnumType<EnumProjetoType>>().Not.Nullable();
            Map(b => b.Enabled).Not.Nullable();
            Map(b => b.TemplateId).Nullable();
            Map(b => b.Subject).Length(500).Not.Nullable();
            Map(b => b.DaysBeforeCheckInJson).Length(2000).Not.Nullable();
            Map(b => b.ExcludedStatusCrcIdsJson).Length(2000).Not.Nullable();
            Map(b => b.SendOnlyToAdimplentes).Not.Nullable();
            Map(b => b.AllCompanies).Not.Nullable();
            Map(b => b.EmpresaIdsJson).Length(2000).Not.Nullable();
            Map(b => b.TemplateSendMode).CustomType<EnumType<EnumTemplateSendMode>>().Not.Nullable();

            Schema("portalohana");
            Table("AutomaticCommunicationConfig");
        }
    }
}

