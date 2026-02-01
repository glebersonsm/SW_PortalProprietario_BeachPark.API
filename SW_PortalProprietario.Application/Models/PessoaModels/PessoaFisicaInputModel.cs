using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class PessoaFisicaInputModel : CreateUpdateModelBase
    {
        public string? Nome { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? EmailPreferencial { get; set; }
        public string? EmailAlternativo { get; set; }
        public EnumTipoPessoa TipoPessoa { get; set; } = EnumTipoPessoa.Fisica;
        public List<PessoaTelefoneInputModel>? Telefones { get; set; }
        public List<PessoaEnderecoInputModel>? Enderecos { get; set; }
        public List<PessoaDocumentoInputModel>? Documentos { get; set; }

    }
}
