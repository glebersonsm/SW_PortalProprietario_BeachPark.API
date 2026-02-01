using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.FrameworkModels
{
    public class RegistroGrupoEmpresaInputModel : CreateUpdateModelBase
    {
        public PessoaJuridicaInputModel? Pessoa { get; set; }
        public EnumStatus Status { get; }

    }
}
