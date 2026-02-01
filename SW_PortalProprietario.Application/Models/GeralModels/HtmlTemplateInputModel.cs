using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class HtmlTemplateInputModel
    {
        public HtmlTemplateInputModel()
        { }
        public int? Id { get; set; }
        public string? Titulo { get; set; }
        public string? Header { get; set; }
        public string? Content { get; set; }
        public string? Consulta { get; set; }
        public EnumHtmlTipoComunicacao? TipoComunicacao { get; set; }
    }
}

