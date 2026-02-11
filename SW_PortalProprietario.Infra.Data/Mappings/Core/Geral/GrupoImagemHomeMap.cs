using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class GrupoImagemHomeMap : ClassMap<GrupoImagemHome>
    {
        public GrupoImagemHomeMap()
        {
            Id(x => x.Id).GeneratedBy.Native("GrupoImagemHome_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.Nome).Length(100);
            Map(b => b.Ordem).Nullable();
            References(b => b.Empresa, "Empresa");

            Schema("portalohana");
            Table("GrupoImagemHome");
        }
    }
}

