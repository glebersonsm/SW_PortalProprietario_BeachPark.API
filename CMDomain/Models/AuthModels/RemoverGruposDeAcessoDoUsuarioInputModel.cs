namespace CMDomain.Models.AuthModels
{
    public class RemoverGruposDeAcessoDoUsuarioInputModel : ModelRequestBase
    {
        public int? UsuarioId { get; set; }
        public List<int> GruposIdsRemover { get; set; } = new List<int>();

    }
}
