namespace CMDomain.Models.AuthModels
{
    public class AdicionarRemoverTipoDocRecPagUsuarioInputModel : ModelRequestBase
    {
        public int? UsuarioId { get; set; }
        public List<TipoDocRecPagInputModel> TiposDocRecPag { get; set; } = new List<TipoDocRecPagInputModel>();

    }
}
