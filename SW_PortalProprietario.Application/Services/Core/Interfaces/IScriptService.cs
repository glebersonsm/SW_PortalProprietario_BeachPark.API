using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IScriptService
    {
        Task<HtmlTemplateResultModel?> GenerateHtmlFromTemplate(HtmlTemplateExecuteModel model);
    }
}
