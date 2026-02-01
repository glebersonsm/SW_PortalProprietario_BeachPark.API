using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IHtmlTemplateService
    {
        Task<HtmlTemplateModel> SaveHtmlTemplate(HtmlTemplateInputModel model);
        Task<DeleteResultModel> DeleteHtmlTemplate(int id);
        Task<List<HtmlTemplateModel>?> Search(SearchHtmlTemplateModel searchModel);
        Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP(GetHtmlValuesModel model);
    }
}
