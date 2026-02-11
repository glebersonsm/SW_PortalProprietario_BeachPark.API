using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Sistema
{
    public class UsuarioGrupoUsuarioMap : ClassMap<UsuarioGrupoUsuario>
    {
        public UsuarioGrupoUsuarioMap()
        {
            Id(x => x.Id).GeneratedBy.Native("UsuarioGrupoUsuario_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);

            Map(b => b.DataHoraCriacao);

            Map(b => b.DataHoraAlteracao)
                .Nullable();

            Map(p => p.UsuarioAlteracao)
                .Nullable();

            References(p => p.Usuario, "Usuario");
            References(p => p.GrupoUsuario, "GrupoUsuario");


            Schema("portalohana");
            Table("UsuarioGrupoUsuario");
        }
    }
}
