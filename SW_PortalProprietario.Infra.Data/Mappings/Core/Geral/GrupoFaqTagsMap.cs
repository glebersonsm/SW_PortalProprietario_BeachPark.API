using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class GrupoFaqTagsMap : ClassMap<GrupoFaqTags>
    {
        public GrupoFaqTagsMap()
        {
            Id(x => x.Id).GeneratedBy.Native("GrupoFaqTags_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            References(x => x.GrupoFaq, "GrupoFaq");
            References(x => x.Tags, "Tags");
            Table("GrupoFaqTags");
        }
    }
}

