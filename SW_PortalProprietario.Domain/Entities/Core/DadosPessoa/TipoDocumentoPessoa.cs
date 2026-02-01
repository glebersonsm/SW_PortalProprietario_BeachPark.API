using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.DadosPessoa
{
    public class TipoDocumentoPessoa : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? Nome { get; set; }
        public virtual string? Mascara { get; set; }
        public virtual EnumSimNao? ExigeOrgaoEmissor { get; set; }
        public virtual EnumSimNao? ExigeDataEmissao { get; set; }
        public virtual EnumSimNao? ExigeDataValidade { get; set; }
        public virtual EnumTiposPessoa? TipoPessoa { get; set; } = EnumTiposPessoa.PessoaFisicaEJuridica;

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (string.IsNullOrEmpty(Nome))
                mensagens.Add($"O Nome do Tipo de Documento deve ser informado");

            if (!ExigeOrgaoEmissor.HasValue)
                mensagens.Add($"O Campo Exige Orgão Emissor do Tipo de Documento deve ser informado");

            if (!ExigeDataEmissao.HasValue)
                mensagens.Add($"O Campo Exige Data Emissao do Tipo de Documento deve ser informado");

            if (!ExigeDataValidade.HasValue)
                mensagens.Add($"O Campo Exige Data Validade do Tipo de Documento deve ser informado");

            if (!TipoPessoa.HasValue)
                mensagens.Add($"O Campo Tipo Pessoa do Tipo de Documento deve ser informado");

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
