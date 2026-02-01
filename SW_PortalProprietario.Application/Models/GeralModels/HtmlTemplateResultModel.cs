namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class HtmlTemplateResultModel : ModelBase
    {
        public HtmlTemplateResultModel()
        { }
        public HtmlTemplateModel? TemplateModelPopulado { get; set; }
        public virtual string? Html { get; set; }
        public virtual string? FilePath { get; set; }

    }
}
