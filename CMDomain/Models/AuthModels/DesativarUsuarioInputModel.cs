namespace CMDomain.Models.AuthModels
{
    public class DesativarUsuarioInputModel : ModelRequestBase
    {
        public int? UsuarioId { get; set; }
        public bool? RemoverTodosOsAcessosDoUsuario { get; set; }

    }
}
