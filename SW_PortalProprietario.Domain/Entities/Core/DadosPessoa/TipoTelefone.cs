namespace SW_PortalProprietario.Domain.Entities.Core.DadosPessoa
{
    public class TipoTelefone : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? Nome { get; set; }
        public virtual string? Mascara { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();
            if (string.IsNullOrEmpty(Nome))
                mensagens.Add($"O Nome do Tipo de Telefone deve ser informado");

            //{0:(##) #####-####}

            if (!string.IsNullOrEmpty(Mascara))
            {
                var baseNumerica = Functions.Helper.ApenasPosicoesComCaracterePesquisado(Mascara, '#');
                if (!baseNumerica.Contains("#"))
                    throw new ArgumentException("Deve ser utilizado o caractere '#' para as possições numéricas da máscara");
            }

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
