namespace CMDomain.Models.AuthModels
{
    public class SearchModuloModel : ModelRequestBase
    {
        public int? ModuloId { get; set; }
        public string? ModuloNome { get; set; }

    }
}
