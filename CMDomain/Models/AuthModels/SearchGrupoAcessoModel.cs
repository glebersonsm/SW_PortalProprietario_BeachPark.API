namespace CMDomain.Models.AuthModels
{
    public class SearchGrupoAcessoModel : ModelRequestBase
    {
        public int? GrupoId { get; set; }
        public int? EmpresaId { get; set; }
        public string? GrupoNome { get; set; }

    }
}
