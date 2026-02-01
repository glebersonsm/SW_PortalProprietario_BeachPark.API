using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Sistema
{
    public class GrupoUsuarioModuloPermissaoMap : ClassMap<GrupoUsuarioModuloPermissao>
    {
        public GrupoUsuarioModuloPermissaoMap()
        {
            Id(x => x.Id).GeneratedBy.Native("GrupoUsuarioModuloPermissao_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            References(p => p.ModuloPermissao, "ModuloPermissao");
            References(p => p.GrupoUsuario, "GrupoUsuario");
            Table("GrupoUsuarioModuloPermissao");
        }
    }
}
