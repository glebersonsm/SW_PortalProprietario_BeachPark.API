using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.DadosPessoa
{
    public class PessoaTelefone : EntityBaseCore, IEntityValidateCore
    {
        public virtual Pessoa? Pessoa { get; set; }
        public virtual TipoTelefone? TipoTelefone { get; set; }
        public virtual string? Numero { get; set; }
        public virtual string? NumeroFormatado { get; set; }
        public virtual EnumSimNao? Preferencial { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Pessoa == null)
                mensagens.Add($"A Pessoa do Telefone deve ser informada");

            if (TipoTelefone == null)
                mensagens.Add($"O Tipo do Telefone deve ser informado");

            if (string.IsNullOrEmpty(Numero))
                mensagens.Add($"O Número do Telefone deve ser informado");

            if (!Preferencial.HasValue)
                mensagens.Add($"Deve ser informado se o Telefone é Preferencial");

            if (TipoTelefone != null && !string.IsNullOrEmpty(TipoTelefone.Mascara))
            {
                var qtdEsperada = TipoTelefone.Mascara.ToCharArray().Count(b => b.ToString() == "#");
                if (qtdEsperada != Numero?.Length)
                    mensagens.Add($"A máscara: '{TipoTelefone.Mascara}' do tipo de telefone: {TipoTelefone.Nome} exige a informação de: {qtdEsperada} caracteres numéricos para sua composição, e foram infomados: {Numero?.Length ?? 0}");

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
