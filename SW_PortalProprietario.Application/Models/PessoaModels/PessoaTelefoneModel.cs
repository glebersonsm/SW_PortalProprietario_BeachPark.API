using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class PessoaTelefoneModel : ModelBase
    {
        public PessoaTelefoneModel()
        { }

        public int? PessoaId { get; set; }
        public int? TipoTelefoneId { get; set; }
        public string? TipoTelefoneNome { get; set; }
        public string? TipoTelefoneMascara { get; set; }
        public string? Numero { get; set; }
        public string? NumeroFormatado { get; set; }
        public EnumSimNao? Preferencial { get; set; }

    }
}

