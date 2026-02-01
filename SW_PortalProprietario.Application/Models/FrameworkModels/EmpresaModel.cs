using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.FrameworkModels
{
    public class EmpresaModel : ModelBase
    {
        public EmpresaModel()
        {

        }

        public string? Codigo { get; set; }
        public PessoaCompletaModel? PessoaEmpresa { get; set; }
        public int? GrupoEmpresaId { get; set; }
        public PessoaCompletaModel? PessoaGrupoEmpresa { get; set; }
        public string? GrupoEmpresaCodigo { get; set; }
        public EnumTipoTributacao? EmpresaRegimeTributacao { get; set; } = EnumTipoTributacao.SimplesNacional;
        public string? NomeCondominio { get; set; }
        public string? CnpjCondominio { get; set; }
        public string? EnderecoCondominio { get; set; }
        public string? NomeAdministradoraCondominio { get; set; }
        public string? CnpjAdministradoraCondominio { get; set; }
        public string? EnderecoAdministradoraCondominio { get; set; }


    }
}
