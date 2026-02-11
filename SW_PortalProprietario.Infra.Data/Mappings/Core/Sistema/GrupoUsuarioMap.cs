using FluentNHibernate.Mapping;
using NHibernate.Type;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Sistema
{
    public class GrupoUsuarioMap : ClassMap<GrupoUsuario>
    {
        public GrupoUsuarioMap()
        {
            Id(x => x.Id).GeneratedBy.Native("GrupoUsuario_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.Nome).Length(100);
            Map(b => b.Status).CustomType<EnumType<EnumStatus>>();

            Schema("portalohana");
            Table("GrupoUsuario");
        }
    }
}
