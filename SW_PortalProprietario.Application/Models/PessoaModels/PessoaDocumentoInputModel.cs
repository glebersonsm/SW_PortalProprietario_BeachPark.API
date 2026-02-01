using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class PessoaDocumentoInputModel : CreateUpdateModelBase
    {
        public virtual int? PessoaId { get; set; }
        public virtual int? TipoDocumentoId { get; set; }
        public string? Numero { get; set; }
        public string? OrgaoEmissor { get; set; }
        public DateTime? DataEmissao { get; set; }
        public DateTime? DataValidade { get; set; }

        public static explicit operator PessoaDocumento(PessoaDocumentoInputModel model)
        {
            return new PessoaDocumento
            {
                Numero = model.Numero,
                OrgaoEmissor = model.OrgaoEmissor,
                DataEmissao = model.DataEmissao,
                DataValidade = model.DataValidade,
                Pessoa = new Domain.Entities.Core.DadosPessoa.Pessoa() { Id = model.PessoaId.GetValueOrDefault() },
                TipoDocumento = new TipoDocumentoPessoa() { Id = model.TipoDocumentoId.GetValueOrDefault() }
            };
        }

    }
}
