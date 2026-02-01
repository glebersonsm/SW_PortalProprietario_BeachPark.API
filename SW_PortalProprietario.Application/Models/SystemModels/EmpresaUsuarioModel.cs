namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class EmpresaUsuarioModel : ModelBase
    {
        public int? UsuarioId { get; set; }
        public string? UsuarioLogin { get; set; }
        public int? EmpresaId { get; set; }
        public string? EmpresaCodigo { get; set; }
        public string? EmpresaNome { get; set; }
        public string? PessoaJuridicaNome { get; set; }
        public int? GrupoEmpresaId { get; set; }
        public string? GrupoEmpresaCodigo { get; set; }
        public string? GrupoEmpresaNome { get; set; }


    }
}
