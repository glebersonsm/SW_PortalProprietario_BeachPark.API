namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class ImagemGrupoImagemHomeModel : ModelBase
    {
        public int? GrupoImagemHomeId { get; set; }
        public string? GrupoImagemHomeName { get; set; }
        public string? Name { get; set; }
        public byte[]? Imagem { get; set; }
        public string? ImagemBase64 => Imagem != null ? Convert.ToBase64String(Imagem) : null;
        public string? NomeBotao { get; set; }
        public string? LinkBotao { get; set; }
        public int? Ordem { get; set; }
        public DateTime? DataInicioVigencia { get; set; }
        public DateTime? DataFimVigencia { get; set; }
        public List<ImagemGrupoImagemHomeTagsModel>? TagsRequeridas { get; set; }
    }
}

