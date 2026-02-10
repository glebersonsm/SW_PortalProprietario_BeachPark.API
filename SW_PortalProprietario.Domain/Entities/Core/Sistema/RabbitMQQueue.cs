using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Sistema
{
    /// <summary>
    /// Entidade para gerenciar configurações de filas RabbitMQ
    /// </summary>
    public class RabbitMQQueue : EntityBaseCore, IEntityValidateCore
    {
        public virtual string Nome { get; set; } = string.Empty;
        public virtual string? Descricao { get; set; }
        public virtual EnumSimNao Ativo { get; set; } = EnumSimNao.Sim;
        public virtual string TipoFila { get; set; } = string.Empty; // "Auditoria", "Log", "Email"
        public virtual string? ExchangeName { get; set; }
        public virtual string? RoutingKey { get; set; }
        public virtual int? PrefetchCount { get; set; }
        public virtual int? ConsumerConcurrency { get; set; }
        public virtual int? RetryAttempts { get; set; }
        public virtual int? RetryDelaySeconds { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (string.IsNullOrWhiteSpace(Nome))
                mensagens.Add("O nome da fila deve ser informado.");

            if (string.IsNullOrWhiteSpace(TipoFila))
                mensagens.Add("O tipo da fila deve ser informado.");

            if (mensagens.Any())
                await Task.FromException(new ArgumentException(string.Join($"{Environment.NewLine}", mensagens.Select(a => a).ToList())));
            else await Task.CompletedTask;
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
