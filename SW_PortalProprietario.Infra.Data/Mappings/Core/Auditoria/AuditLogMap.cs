using FluentNHibernate.Mapping;
using NHibernate.Type;
using SW_PortalProprietario.Domain.Entities.Core.Auditoria;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Auditoria
{
    public class AuditLogMap : ClassMap<AuditLog>
    {
        public AuditLogMap()
        {
            Id(x => x.Id).GeneratedBy.Native("AuditLog_");
            Map(x => x.ObjectGuid).Length(100).Nullable();
            Map(p => p.UsuarioCriacao).Nullable();
            Map(b => b.DataHoraCriacao).Nullable();
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();

            Map(b => b.EntityType).Length(200).Not.Nullable().Index("IX_AuditLog_EntityType_EntityId");
            Map(b => b.EntityId).Not.Nullable().Index("IX_AuditLog_EntityType_EntityId");
            Map(b => b.Action).CustomType<EnumType<EnumAuditAction>>().Not.Nullable().Index("IX_AuditLog_Action");
            Map(b => b.UserId).Nullable().Index("IX_AuditLog_UserId_Timestamp");
            Map(b => b.UserName).Length(200).Nullable();
            Map(b => b.Timestamp).Not.Nullable().Index("IX_AuditLog_UserId_Timestamp").Index("IX_AuditLog_Timestamp");
            Map(b => b.IpAddress).Length(50).Nullable();
            Map(b => b.UserAgent).Length(500).Nullable();
            Map(b => b.ChangesJson).Length(10000).Not.Nullable();
            Map(b => b.EntityDataJson).Length(50000).Nullable();

            Table("AuditLog");
        }
    }
}

