using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Framework;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Framework
{
    public class ModuloEmpresaMap : ClassMap<ModuloEmpresa>
    {
        public ModuloEmpresaMap()
        {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);

            Map(b => b.DataHoraCriacao);

            Map(b => b.DataHoraAlteracao)
                .Nullable();

            Map(p => p.UsuarioAlteracao)
                .Nullable();

            References(p => p.Empresa, "Empresa");
            References(p => p.Modulo, "Modulo");

            Schema("portalohana");
            Table("ModuloEmpresa");
        }
    }
}
