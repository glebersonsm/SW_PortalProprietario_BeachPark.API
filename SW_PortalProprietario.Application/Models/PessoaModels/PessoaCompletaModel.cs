using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class PessoaCompletaModel : ModelBase
    {
        public PessoaCompletaModel()
        { }

        public DateTime? DataNascimento { get; set; }
        public DateTime? DataAbertura { get; set; }
        public string? Nome { get; set; }
        public string? NomeFantasia { get; set; }
        public string? EmailPreferencial { get; set; }
        public string? EmailAlternativo { get; set; }
        public EnumTipoPessoa TipoPessoa { get; set; }
        public EnumTipoTributacao? RegimeTributario { get; set; }

        public List<PessoaDocumentoModel>? Documentos { get; set; }
        public List<PessoaEnderecoModel>? Enderecos { get; set; }
        public List<PessoaTelefoneModel>? Telefones { get; set; }

    }
}

