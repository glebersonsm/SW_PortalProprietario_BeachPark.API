using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class PessoaEnderecoInputModel : CreateUpdateModelBase
    {
        public int? PessoaId { get; set; }
        public int? TipoEnderecoId { get; set; }
        public int? CidadeId { get; set; }
        public string? Logradouro { get; set; }
        public string? Bairro { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Cep { get; set; }
        public EnumSimNao? Preferencial { get; set; }


        public static explicit operator PessoaEndereco(PessoaEnderecoInputModel model)
        {
            return new PessoaEndereco
            {
                Logradouro = model.Logradouro,
                Bairro = model.Bairro,
                Numero = model.Numero,
                Complemento = model.Complemento,
                Pessoa = new Pessoa() { Id = model.PessoaId.GetValueOrDefault() },
                TipoEndereco = new TipoEndereco() { Id = model.TipoEnderecoId.GetValueOrDefault() },
                Cidade = new Cidade() { Id = model.CidadeId.GetValueOrDefault() }
            };
        }

    }
}
