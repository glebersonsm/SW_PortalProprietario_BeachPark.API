namespace SW_PortalProprietario.Application.Models.FrameworkModels
{
    public class EmpresaGrupoEmpresaInputModel : CreateUpdateModelBase
    {
        public int? GrupoEmpresaId { get; set; }
        public int? GrupoEmpresaPessoaId { get; set; }
        public string? GrupoEmpresaCodigo { get; set; }
        public string? GrupoEmpresaNome { get; set; }
        public int? EmpresaId { get; set; }
        public int? EmpresaPessoaId { get; set; }
        public string? EmpresaCodigo { get; set; }
        public string? EmpresaNome { get; set; }

    }
}
