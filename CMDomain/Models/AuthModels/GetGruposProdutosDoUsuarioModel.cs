namespace CMDomain.Models.AuthModels
{
    public class GetGruposProdutosDoUsuarioModel : ModelRequestBase
    {
        public string? UsuarioNome { get; set; }
        public int? UsuarioId { get; set; }

    }
}
