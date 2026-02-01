namespace CMDomain.Models.AuthModels
{
    public class AdicionarRemoverAlmoxarifadoUsuarioInputModel : ModelRequestBase
    {
        public int? UsuarioId { get; set; }
        public List<AlmoxarifadoPessoaInputModel> Almoxarifados { get; set; } = new List<AlmoxarifadoPessoaInputModel>();

    }
}
