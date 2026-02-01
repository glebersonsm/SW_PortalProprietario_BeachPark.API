namespace CMDomain.Models.AuthModels
{
    public class AdicionarRemoverGrupoProdutoUsuarioInputModel : ModelRequestBase
    {
        public int? UsuarioId { get; set; }
        public List<GrupoProdutoXPessoaInputModel> GruposProduto { get; set; } = new List<GrupoProdutoXPessoaInputModel>();

    }
}
