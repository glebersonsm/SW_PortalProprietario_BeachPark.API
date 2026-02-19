using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class GrupoFaqMap : ClassMap<GrupoFaq>
    {
        public GrupoFaqMap()
        {
            Id(x => x.Id).GeneratedBy.Native("GrupoFaq_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            References(p => p.Empresa, "Empresa");
            Map(b => b.Nome).Length(200);
            Map(b => b.Disponivel).CustomType<EnumSimNao>();
            Map(b => b.Ordem).Nullable();
            References(b => b.GrupoFaqPai, "IdGrupoFaqPai").Nullable();

            Schema("portalohana");
            Table("GrupoFaq");
        }
    }
}

