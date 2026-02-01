namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class ImageGroupImageModel : ModelBase
    {
        public int? ImageGroupId { get; set; }
        public string? ImageGroupName { get; set; }
        public string? Name { get; set; }
        public byte[]? Imagem { get; set; }
        public string? ImagemBase64 => Imagem != null ? Convert.ToBase64String(Imagem) : null;
        public string? NomeBotao { get; set; }
        public string? LinkBotao { get; set; }
        public DateTime? DataInicioVigencia { get; set; }
        public string? DataInicioVigenciaStr => DataInicioVigencia?.ToString("yyyy-MM-dd");
        public DateTime? DataFimVigencia { get; set; }
        public string? DataFimVigenciaStr => DataFimVigencia?.ToString("yyyy-MM-dd");
        public int? Ordem { get; set; }

        public List<ImagemGrupoImagemTagsModel>? TagsRequeridas { get; set; }
        public string? ServerAddress { get; internal set; }
    }
}
