using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Sistema
{
    public class RabbitMQQueueMap : ClassMap<RabbitMQQueue>
    {
        public RabbitMQQueueMap()
        {
            Id(x => x.Id).GeneratedBy.Native("RabbitMQQueue_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();

            Map(b => b.Nome).Length(200).Not.Nullable();
            Map(b => b.Descricao).Length(500);
            Map(b => b.Ativo).CustomType<EnumSimNao>().Not.Nullable();
            Map(b => b.TipoFila).Length(50).Not.Nullable();
            Map(b => b.ExchangeName).Length(200);
            Map(b => b.RoutingKey).Length(200);
            Map(b => b.PrefetchCount);
            Map(b => b.ConsumerConcurrency);
            Map(b => b.RetryAttempts);
            Map(b => b.RetryDelaySeconds);

            Schema("portalohana");
            Table("RabbitMQQueue");
        }
    }
}
