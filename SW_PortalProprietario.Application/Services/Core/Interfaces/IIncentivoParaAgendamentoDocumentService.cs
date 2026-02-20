using SW_PortalProprietario.Application.Models.DocumentTemplates;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces;

/// <summary>
/// Interface para geraÃ§Ã£o de documentos de incentivo para agendamento
/// Diferente de IIncentivoParaAgendamentoService em Application.Interfaces que Ã© para comunicaÃ§Ã£o automÃ¡tica
/// </summary>
public interface IIncentivoParaAgendamentoDocumentService
{
    Task<IncentivoAgendamentoDocumentResultModel> GerarDocumentoIncentivoAsync(int contratoId, int anoReferencia);
    Task<DadosIncentivoAgendamentoModel?> ObterDadosIncentivoAsync(int contratoId, int anoReferencia);
    IReadOnlyCollection<PlaceholderDescriptionIncentivo> ListarPlaceholders();
}
