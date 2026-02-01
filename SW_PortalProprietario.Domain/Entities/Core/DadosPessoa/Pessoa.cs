using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.DadosPessoa
{
    public class Pessoa : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? Nome { get; set; }
        public virtual string? NomeFantasia { get; set; }
        public virtual string? EmailPreferencial { get; set; }
        public virtual string? EmailAlternativo { get; set; }
        public virtual DateTime? DataNascimento { get; set; }
        public virtual DateTime? DataAbertura { get; set; }
        public virtual DateTime? DataEncerramento { get; set; }
        public virtual EnumTipoPessoa? TipoPessoa { get; set; }
        public virtual EnumTipoTributacao? RegimeTributario { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (string.IsNullOrEmpty(Nome))
                mensagens.Add($"O Nome da pessoa deve ser informado");

            if (TipoPessoa != EnumTipoPessoa.Fisica && TipoPessoa != EnumTipoPessoa.Juridica)
            {
                mensagens.Add($"O Tipo da pessoa deve ser informado");
            }

            if (TipoPessoa == EnumTipoPessoa.Juridica)
            {
                if (string.IsNullOrEmpty(NomeFantasia))
                    mensagens.Add($"O Nome Fantasia da pessoa deve ser informado");

                if (!RegimeTributario.HasValue)
                    mensagens.Add($"O Regime tributário pessoa deve ser informado");
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
