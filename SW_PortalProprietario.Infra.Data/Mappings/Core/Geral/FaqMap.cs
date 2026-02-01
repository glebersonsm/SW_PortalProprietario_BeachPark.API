using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class FaqMap : ClassMap<Faq>
    {
        public FaqMap()
        {
            Id(x => x.Id).GeneratedBy.Native("Faq_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            References(p => p.GrupoFaq, "GrupoFaq");
            Map(b => b.Pergunta).Length(2000);
            Map(b => b.Resposta).Length(2000);
            Map(b => b.Disponivel).CustomType<EnumSimNao>();
            Map(b => b.Ordem).Nullable();
            Table("Faq");
        }
    }
}

