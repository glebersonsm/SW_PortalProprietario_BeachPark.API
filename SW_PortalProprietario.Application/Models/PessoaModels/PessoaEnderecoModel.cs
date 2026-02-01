using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class PessoaEnderecoModel : ModelBase
    {
        public PessoaEnderecoModel()
        { }

        public int? PessoaId { get; set; }
        public int? CidadeId { get; set; }
        public string? CidadeNome { get; set; }
        public int? EstadoId { get; set; }
        public string? EstadoSigla { get; set; }
        public string? EstadoNome { get; set; }
        public int? TipoEnderecoId { get; set; }
        public string? TipoEnderecoNome { get; set; }
        public string? Numero { get; set; }
        public string? Logradouro { get; set; }
        public string? Bairro { get; set; }
        public string? Complemento { get; set; }
        public string? Cep { get; set; }
        public EnumSimNao? Preferencial { get; set; }

    }
}

