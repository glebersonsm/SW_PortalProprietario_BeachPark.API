using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.FrameworkModels
{
    public class AlteracaoGrupoEmpresaInputModel : CreateUpdateModelBase
    {
        public string? Codigo { get; set; }
        public EnumStatus Status { get; }
        public PessoaJuridicaInputModel? Pessoa { get; set; }

    }
}
