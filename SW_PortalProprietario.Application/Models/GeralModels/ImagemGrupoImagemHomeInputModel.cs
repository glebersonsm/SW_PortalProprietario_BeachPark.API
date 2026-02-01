using Microsoft.AspNetCore.Http;

namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class ImagemGrupoImagemHomeInputModel
    {
        public int? Id { get; set; }
        public int? GrupoImagemHomeId { get; set; }
        public string? Name { get; set; }
        public IFormFile? Imagem { get; set; }
        public string? NomeBotao { get; set; }
        public string? LinkBotao { get; set; }
        public int? Ordem { get; set; }
        public DateTime? DataInicioVigencia { get; set; }
        public DateTime? DataFimVigencia { get; set; }
        public List<int>? TagsRequeridas { get; set; }
        public bool? RemoverTagsNaoEnviadas { get; set; } = false;
    }
}

