using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class HtmlTemplate : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? Titulo { get; set; }
        public virtual string? Header { get; set; }
        public virtual string? Content { get; set; }
        public virtual string? Consulta { get; set; }
        public virtual string? ColunasDeRetorno { get; set; }
        public virtual string? ParametrosConsulta { get; set; }
        public virtual EnumHtmlTipoComunicacao? TipoComunicacao { get; set; }
        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (string.IsNullOrEmpty(Titulo))
                mensagens.Add($"O Titulo deve ser informado");

            if (string.IsNullOrEmpty(Header))
                mensagens.Add($"O Header deve ser informado");

            if (string.IsNullOrEmpty(Content))
                mensagens.Add($"O Content deve ser informado");

            if (!TipoComunicacao.HasValue)
                mensagens.Add($"O Tipo de comunicação deve ser informado");

            if (mensagens.Any())
                await Task.FromException(new ArgumentException(string.Join($"{Environment.NewLine}", mensagens.Select(a => a).ToList())));
            else await Task.CompletedTask;
        }
    }
}
