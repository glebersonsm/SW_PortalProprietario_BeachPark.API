using FluentNHibernate.Mapping;
using NHibernate.Type;
using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Framework
{
    public class GrupoEmpresaMap : ClassMap<GrupoEmpresa>
    {
        public GrupoEmpresaMap()
        {
            Id(x => x.Id).GeneratedBy.Native("GrupoEmpresa_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);

            Map(b => b.DataHoraCriacao);

            Map(b => b.DataHoraAlteracao)
                .Nullable();

            Map(p => p.UsuarioAlteracao)
                .Nullable();

            Map(b => b.Codigo)
                .Length(20).Unique();
            References(p => p.Pessoa, "Pessoa").UniqueKey("UK_GrupoEmpresaPessoa");
            Map(b => b.Status).CustomType<EnumType<EnumStatus>>();

            Table("GrupoEmpresa");
        }
    }
}
