using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.PessoaModels
{
    public class PessoaSearchModel : SearchPadraoModel
    {
        public EnumTipoPessoa? Tipo { get; set; }
        public string? Documento { get; set; }
        public string? Email { get; set; }
        public bool? CarregarCompleto { get; set; } = false;
    }
}
