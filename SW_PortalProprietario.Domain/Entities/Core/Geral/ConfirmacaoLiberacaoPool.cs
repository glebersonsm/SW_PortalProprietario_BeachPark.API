using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class ConfirmacaoLiberacaoPool : EntityBaseCore, IEntityValidateCore
    {
        public virtual Empresa? Empresa { get; set; }
        public virtual EnumSimNao? LiberacaoDiretaPeloCliente { get; set; } = EnumSimNao.Sim;
        public virtual string? CodigoEnviadoAoCliente { get; set; }
        public virtual EnumSimNao? LiberacaoConfirmada { get; set; } = EnumSimNao.Não;
        public virtual DateTime? DataConfirmacao { get; set; }
        public virtual Email? Email { get; set; }
        public virtual string? Tentativas { get; set; }
        public virtual int? AgendamentoId { get; set; }
        public virtual int? NovoAgendamentoId { get; set; }
        public virtual string? Banco { get; set; }
        public virtual string? Conta { get; set; }
        public virtual string? ContaDigito { get; set; }
        public virtual string? Agencia { get; set; }
        public virtual string? AgenciaDigito { get; set; }
        public virtual string? ChavePix { get; set; }
        public virtual string? Tipo { get; set; }
        public virtual string? Variacao { get; set; }
        public virtual string? TipoConta { get; set; }
        public virtual string? Preferencial { get; set; }
        public virtual string? TipoChavePix { get; set; }
        public virtual string? IdCidade { get; set; }


        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Empresa == null)
                mensagens.Add("A Empresa deve ser informada");

            //if (string.IsNullOrEmpty(CodigoEnviadoAoCliente) && LiberacaoDiretaPeloCliente.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
            //    mensagens.Add($"O código enviado ao cliente deve ser informada");

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
