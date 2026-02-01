using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.DadosPessoa
{
    public class PessoaEndereco : EntityBaseCore, IEntityValidateCore
    {
        public virtual Pessoa? Pessoa { get; set; }
        public virtual TipoEndereco? TipoEndereco { get; set; }
        public virtual Cidade? Cidade { get; set; }
        public virtual string? Logradouro { get; set; }
        public virtual string? Bairro { get; set; }
        public virtual string? Numero { get; set; }
        public virtual string? Complemento { get; set; }
        public virtual string? Cep { get; set; }
        public virtual EnumSimNao? Preferencial { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Pessoa == null)
                mensagens.Add($"A Pessoa do Endereço deve ser informada");

            if (TipoEndereco == null)
                mensagens.Add($"O Tipo do Endereço deve ser informado");

            if (Cidade == null)
                mensagens.Add($"A Cidade do Endereço deve ser informada");

            if (string.IsNullOrEmpty(Bairro))
                mensagens.Add($"Deve ser informado o Bairro do Endereço");

            if (string.IsNullOrEmpty(Logradouro))
                mensagens.Add($"Deve ser informado o Logradouro do Endereço");

            if (!Preferencial.HasValue)
                mensagens.Add($"Deve ser informado se o Endereço é Preferencial");

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
