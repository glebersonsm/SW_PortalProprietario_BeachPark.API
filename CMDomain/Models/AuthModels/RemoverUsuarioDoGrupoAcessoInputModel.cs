namespace CMDomain.Models.AuthModels
{
    public class RemoverUsuarioDoGrupoAcessoInputModel : ModelRequestBase
    {
        public int? GrupoId { get; set; }
        public List<int> UsuarioIdsRemover { get; set; } = new List<int>();

    }
}
