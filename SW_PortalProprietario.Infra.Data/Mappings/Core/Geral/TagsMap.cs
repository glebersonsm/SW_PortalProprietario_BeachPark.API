using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class TagsMap : ClassMap<Tags>
    {
        public TagsMap()
        {
            Id(x => x.Id).GeneratedBy.Native("Tags_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.Nome).Length(200);
            Map(b => b.Path).Length(200);
            References(x => x.Parent, "Parent");

            Schema("portalohana");
            Table("Tags");
        }
    }
}

