using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class ImagemGrupoImagemTagsMap : ClassMap<ImagemGrupoImagemTags>
    {
        public ImagemGrupoImagemTagsMap()
        {
            Id(x => x.Id).GeneratedBy.Native("ImagemGrupoImagemTags_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.DataHoraRemocao).Nullable();
            Map(p => p.UsuarioRemocao).Nullable();

            References(x => x.ImagemGrupoImagem, "ImagemGrupoImagem");
            References(x => x.Tags, "Tags");

            Schema("portalohana");
            Table("ImagemGrupoImagemTags");
        }
    }
}

