using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class GrupoImagemHomeTagsMap : ClassMap<GrupoImagemHomeTags>
    {
        public GrupoImagemHomeTagsMap()
        {
            Id(x => x.Id).GeneratedBy.Native("GrupoImagemHomeTags_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(p => p.UsuarioRemocao).Nullable();
            Map(p => p.DataHoraRemocao).Nullable();
            References(x => x.GrupoImagemHome, "GrupoImagemHome");
            References(x => x.Tags, "Tags");

            Schema("portalohana");
            Table("GrupoImagemHomeTags");
        }
    }
}

