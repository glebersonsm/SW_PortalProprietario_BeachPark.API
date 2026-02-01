namespace SW_PortalProprietario.Domain.Entities.Core.Financeiro
{
    public class PaymentCardTokenizedAttempt : EntityBaseCore, IEntityValidateCore
    {
        public virtual CardTokenized? CardTokenized { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual string? DadosEnviados { get; set; }
        public virtual string? Retorno { get; set; }
        public virtual string? RetornoAmigavel { get; set; }
        public virtual int? EmpresaLegadoId { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (CardTokenized == null)
                mensagens.Add($"Deve ser informado o CardTokenized");

            if (Valor.GetValueOrDefault(0) <= 0)
                mensagens.Add($"O valor deve ser informado e maior que zero.");

            if (string.IsNullOrEmpty(DadosEnviados))
                mensagens.Add($"O campo DadosEnviados deve ser informado");

            if (string.IsNullOrEmpty(Retorno))
                mensagens.Add($"O Retorno deve ser informado");

            if (string.IsNullOrEmpty(RetornoAmigavel))
                mensagens.Add($"O Retorno amigável deve ser informado");

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
