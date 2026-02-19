using SW_PortalProprietario.Application.Models.DocumentTemplates;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces;

public interface IDocumentTemplateService
{
    Task<DocumentTemplateModel> CreateAsync(DocumentTemplateUploadInputModel model, int usuarioId);
    Task<DocumentTemplateModel> UpdateAsync(DocumentTemplateUploadInputModel model, int usuarioId);
    Task<DocumentTemplateModel> UploadAsync(DocumentTemplateUploadInputModel model, int usuarioId);
    Task<DocumentTemplateModel?> GetActiveTemplateAsync(EnumDocumentTemplateType? templateType, int? templateId = null);
    Task<IReadOnlyCollection<DocumentTemplateSummaryModel>> ListAsync(EnumDocumentTemplateType? templateType = null);
    Task<string?> GetTemplateContentHtmlAsync(EnumDocumentTemplateType? templateType, int? templateId = null);
    Task<bool> DeleteAsync(int templateId, int usuarioId);
}

