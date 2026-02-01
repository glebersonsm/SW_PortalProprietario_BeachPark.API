using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class Email : EntityBaseCore, IEntityValidateCore
    {
        public virtual Empresa? Empresa { get; set; }
        public virtual string? Assunto { get; set; }
        public virtual string? Destinatario { get; set; }
        public virtual string? ConteudoEmail { get; set; }
        public virtual EnumSimNao? Enviado { get; set; }

        public virtual EnumSimNao? NaFila { get; set; }
        public virtual DateTime? DataHoraEnvio { get; set; }
        public virtual string? ErroEnvio { get; set; }
        public virtual IList<EmailAnexo> Anexos { get; set; } = new List<EmailAnexo>();
        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Enviado == EnumSimNao.Sim)
                NaFila = EnumSimNao.Não;

            if (string.IsNullOrEmpty(Assunto))
                mensagens.Add("O Assunto deve ser informado no email.");

            if (string.IsNullOrEmpty(Destinatario))
                mensagens.Add($"O Destinatário deve ser informado no email");

            if (string.IsNullOrEmpty(ConteudoEmail))
                mensagens.Add($"O Conteúdo do email deve ser informado");

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
