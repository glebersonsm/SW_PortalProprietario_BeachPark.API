namespace CMDomain.Models.AuthModels
{
    public class AdicionarGruposDeAcessoAoUsuarioInputModel : ModelRequestBase
    {
        public int? UsuarioId { get; set; }
        public List<int> GruposIdsAdicionar { get; set; } = new List<int>();

    }
}
