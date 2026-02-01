using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models
{
    public class SearchPadraoComTipoPessoaModel : SearchPadraoModel
    {
        public EnumTiposPessoa TipoPessoa { get; set; }

    }
}
