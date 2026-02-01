namespace CMDomain.Models.AuthModels
{
    public class AdicionarUsuarioNoGrupoAcessoInputModel : ModelRequestBase
    {
        public int? GrupoId { get; set; }
        public List<int> UsuariosIdsAdicionar { get; set; } = new List<int>();

    }
}
