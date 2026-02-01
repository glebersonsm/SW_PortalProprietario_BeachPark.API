using SW_PortalProprietario.Domain.Entities.Core.Framework;

namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class HistoricoRetiradaPool : EntityBaseCore, IEntityValidateCore
    {
        public virtual Empresa? Empresa { get; set; }
        public virtual int? AgendamentoId { get; set; }
        public virtual int? NovoAgendamentoId { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Empresa == null)
                mensagens.Add("A Empresa deve ser informada");

            if (AgendamentoId.GetValueOrDefault(0) == 0)
                mensagens.Add($"O Id do agendamento deve ser informado.");

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
