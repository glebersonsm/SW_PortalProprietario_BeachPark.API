using Microsoft.AspNetCore.Http;

namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class ImageGroupImageInputModel
    {
        public int? Id { get; set; }
        public int? ImageGroupId { get; set; }
        public string? Name { get; set; }
        public bool? RemoverTagsNaoEnviadas { get; set; } = false;
        public List<int>? TagsRequeridas { get; set; }
        public IFormFile? Imagem { get; set; }
        public string? NomeBotao { get; set; }
        public string? LinkBotao { get; set; }
        public DateTime? DataInicioVigencia { get; set; }
        public DateTime? DataFimVigencia { get; set; }
        public int? Ordem { get; set; }

    }
}
