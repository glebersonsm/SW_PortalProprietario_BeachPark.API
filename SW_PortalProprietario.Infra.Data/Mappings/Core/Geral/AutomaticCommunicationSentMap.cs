using FluentNHibernate.Mapping;
using NHibernate.Type;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class AutomaticCommunicationSentMap : ClassMap<AutomaticCommunicationSent>
    {
        public AutomaticCommunicationSentMap()
        {
            Id(x => x.Id).GeneratedBy.Native("AutomaticCommunicationSent_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();

            Map(b => b.CommunicationType).Length(100).Not.Nullable();
            Map(b => b.ReservaId).Not.Nullable();
            Map(b => b.DaysBeforeCheckIn).Not.Nullable();

            Map(b => b.FrAtendimentoVendaId);

            Map(b => b.DataCheckIn).Not.Nullable();
            Map(b => b.DataEnvio).Not.Nullable();
            Map(b => b.EmailId).Nullable();
            Map(b => b.EmpreendimentoTipo).CustomType<EnumType<EnumProjetoType>>().Not.Nullable();

            Table("AutomaticCommunicationSent");
        }
    }
}

