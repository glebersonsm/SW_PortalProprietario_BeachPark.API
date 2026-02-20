using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.DadosPessoa
{
    public class PessoaDocumento : EntityBaseCore, IEntityValidateCore
    {
        public virtual Pessoa? Pessoa { get; set; }
        public virtual TipoDocumentoPessoa? TipoDocumento { get; set; }
        public virtual string? Numero { get; set; }
        public virtual string? ValorNumerico { get; set; }
        public virtual string? NumeroFormatado { get; set; }
        public virtual string? OrgaoEmissor { get; set; }
        public virtual DateTime? DataEmissao { get; set; }
        public virtual DateTime? DataValidade { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Pessoa == null)
                mensagens.Add($"A Pessoa do Documento deve ser informada");

            if (TipoDocumento == null)
                mensagens.Add($"O Tipo do Documento deve ser informado");

            if (string.IsNullOrEmpty(Numero))
                mensagens.Add($"O Número do documento deve ser informado");

            if (string.IsNullOrEmpty(OrgaoEmissor) && TipoDocumento?.ExigeOrgaoEmissor.GetValueOrDefault(EnumSimNao.Nao) == EnumSimNao.Sim)
                mensagens.Add($"Deve ser informado o Orgão emissor no Documento");

            if (DataEmissao.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue && TipoDocumento?.ExigeDataEmissao.GetValueOrDefault(EnumSimNao.Nao) == EnumSimNao.Sim)
                mensagens.Add($"Deve ser informada a Data de Emissão no Documento");

            if (DataValidade.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue && TipoDocumento?.ExigeDataValidade.GetValueOrDefault(EnumSimNao.Nao) == EnumSimNao.Sim)
                mensagens.Add($"Deve ser informada a Data de Validade no Documento");


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
