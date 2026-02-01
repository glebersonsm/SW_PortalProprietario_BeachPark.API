using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.FrameworkModels
{
    public class GrupoEmpresaModel : ModelBase
    {
        public PessoaCompletaModel? Pessoa { get; set; }
        public string? Codigo { get; set; }
        public EnumStatus? Status { get; set; } = EnumStatus.Ativo;
        public List<EmpresaModel>? Empresas { get; set; }

    }
}
