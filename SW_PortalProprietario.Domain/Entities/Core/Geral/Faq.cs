using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class Faq : EntityBaseCore, IEntityValidateCore
    {
        public virtual GrupoFaq? GrupoFaq { get; set; }
        public virtual string? Pergunta { get; set; }
        public virtual string? Resposta { get; set; }
        public virtual EnumSimNao? Disponivel { get; set; }
        public virtual int? Ordem { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (GrupoFaq == null)
                mensagens.Add("O GrupoFaq deve ser informado");

            if (string.IsNullOrEmpty(Pergunta))
                mensagens.Add($"A Pergunta deve ser informada");

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
