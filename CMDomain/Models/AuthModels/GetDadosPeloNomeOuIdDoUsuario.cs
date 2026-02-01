namespace CMDomain.Models.AuthModels
{
    public class GetDadosPeloNomeOuIdDoUsuario : ModelRequestBase
    {
        public string? UsuarioNome { get; set; }
        public int? UsuarioId { get; set; }

    }
}
