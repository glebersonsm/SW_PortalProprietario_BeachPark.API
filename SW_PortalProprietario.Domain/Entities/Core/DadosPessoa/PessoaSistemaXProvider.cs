namespace SW_PortalProprietario.Domain.Entities.Core.DadosPessoa
{
    public class PessoaSistemaXProvider : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? NomeProvider { get; set; }
        public virtual string? PessoaSistema { get; set; }
        public virtual string? PessoaProvider { get; set; }
        public virtual string? TokenResult { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (string.IsNullOrEmpty(NomeProvider))
                mensagens.Add($"O NomeProvider deve ser informado");

            if (string.IsNullOrEmpty(PessoaProvider))
                mensagens.Add($"O valor de PessoaProvider deve ser informado");

            if (string.IsNullOrEmpty(PessoaSistema))
                mensagens.Add($"O valor de PessoaSistema deve ser informado");


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
