using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class PessoaEnderecoUpdateModel : CreateUpdateModelBase
    {
        public string? Numero { get; set; }
        public string? Logradouro { get; set; }
        public string? Bairro { get; set; }
        public string? Complemento { get; set; }
        public string? Cep { get; set; }
        public EnumSimNao? Preferencial { get; set; }
        public int? PessoaId { get; set; }
        public virtual int? TipoEnderecoId { get; set; }
    }
}
