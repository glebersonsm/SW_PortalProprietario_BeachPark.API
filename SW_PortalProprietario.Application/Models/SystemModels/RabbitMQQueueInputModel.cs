using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class RabbitMQQueueInputModel
    {
        public int? Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public EnumSimNao Ativo { get; set; } = EnumSimNao.Sim;
        public string TipoFila { get; set; } = string.Empty;
        public string? ExchangeName { get; set; }
        public string? RoutingKey { get; set; }
        public int? PrefetchCount { get; set; }
        public int? ConsumerConcurrency { get; set; }
        public int? RetryAttempts { get; set; }
        public int? RetryDelaySeconds { get; set; }
    }
}
