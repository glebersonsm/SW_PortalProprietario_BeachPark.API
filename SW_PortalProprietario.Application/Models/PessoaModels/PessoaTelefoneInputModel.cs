using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class PessoaTelefoneInputModel : CreateUpdateModelBase
    {
        public int? PessoaId { get; set; }
        public int? TipoTelefoneId { get; set; }
        public string? Numero { get; set; }
        public EnumSimNao? Preferencial { get; set; }

    }
}
