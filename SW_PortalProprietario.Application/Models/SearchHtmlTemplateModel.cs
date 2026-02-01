using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models
{
    public class SearchHtmlTemplateModel
    {
        public int? Id { get; set; }
        public string? Titulo { get; set; }
        public EnumHtmlTipoComunicacao? TipoComunicacao { get; set; }

    }
}
