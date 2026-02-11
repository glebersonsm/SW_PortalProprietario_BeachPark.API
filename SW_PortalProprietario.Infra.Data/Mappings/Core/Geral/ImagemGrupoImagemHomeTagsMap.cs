using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class ImagemGrupoImagemHomeTagsMap : ClassMap<ImagemGrupoImagemHomeTags>
    {
        public ImagemGrupoImagemHomeTagsMap()
        {
            Id(x => x.Id).GeneratedBy.Native("ImagemGrupoImagemHomeTags_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            References(x => x.ImagemGrupoImagemHome, "ImagemGrupoImagemHome");
            References(x => x.Tags, "Tags");

            Schema("portalohana");
            Table("ImagemGrupoImagemHomeTags");
        }
    }
}

