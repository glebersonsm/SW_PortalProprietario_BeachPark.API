using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Framework;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Framework
{
    public class ModuloPermissaoMap : ClassMap<ModuloPermissao>
    {
        public ModuloPermissaoMap()
        {
            Id(x => x.Id).GeneratedBy.Native("ModuloPermissao_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            References(b => b.Modulo, "Modulo");
            References(b => b.Permissao, "Permissao");
            Table("ModuloPermissao");
        }
    }
}
