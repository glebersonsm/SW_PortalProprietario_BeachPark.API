using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class GrupoImagemTagsMap : ClassMap<GrupoImagemTags>
    {
        public GrupoImagemTagsMap()
        {
            Id(x => x.Id).GeneratedBy.Native("GrupoImagemTags_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(p => p.UsuarioRemocao).Nullable();
            Map(p => p.DataHoraRemocao).Nullable();
            References(x => x.GrupoImagem, "GrupoImagem");
            References(x => x.Tags, "Tags");
            Table("GrupoImagemTags");
        }
    }
}

