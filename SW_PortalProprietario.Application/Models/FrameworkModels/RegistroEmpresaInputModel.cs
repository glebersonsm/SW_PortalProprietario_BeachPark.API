using SW_PortalProprietario.Application.Models.PessoaModels;

namespace SW_PortalProprietario.Application.Models.FrameworkModels
{
    public class RegistroEmpresaInputModel : CreateUpdateModelBase
    {
        public PessoaJuridicaInputModel? Pessoa { get; set; }
        public int? GrupoEmpresaId { get; set; }
        public string? NomeCondominio { get; set; }
        public string? CnpjCondominio { get; set; }
        public string? EnderecoCondominio { get; set; }
        public string? CidadeCondominio { get; set; }
        public string? EstadoCondominio { get; set; }
        public string? NomeAdministradoraCondominio { get; set; }
        public string? CnpjAdministradoraCondominio { get; set; }
        public string? EnderecoAdministradoraCondominio { get; set; }
        public string? CidadeAdministradoraCondominio { get; set; }
        public string? EstadoAdministradoraCondominio { get; set; }


    }
}
