using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class PessoaJuridicaInputModel : CreateUpdateModelBase
    {
        public string? RazaoSocial { get; set; }
        public string? NomeFantasia { get; set; }
        public DateTime? DataAbertura { get; set; }
        public string? EmailPreferencial { get; set; }
        public string? EmailAlternativo { get; set; }
        public EnumTipoPessoa TipoPessoa { get; set; } = EnumTipoPessoa.Juridica;

        public EnumTipoTributacao? RegimeTributario { get; set; }
        public List<PessoaTelefoneInputModel>? Telefones { get; set; }
        public List<PessoaEnderecoInputModel>? Enderecos { get; set; }
        public List<PessoaDocumentoInputModel>? Documentos { get; set; }


        public static explicit operator Pessoa(PessoaJuridicaInputModel model)
        {
            return new Domain.Entities.Core.DadosPessoa.Pessoa
            {
                Id = model.Id.GetValueOrDefault(),
                DataAbertura = model.DataAbertura,
                EmailAlternativo = model.EmailAlternativo,
                EmailPreferencial = model.EmailPreferencial,
                Nome = model.RazaoSocial,
                NomeFantasia = model.NomeFantasia,
                RegimeTributario = model.RegimeTributario
            };
        }

    }
}
