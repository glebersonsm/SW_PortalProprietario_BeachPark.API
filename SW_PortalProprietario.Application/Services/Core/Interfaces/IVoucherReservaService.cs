using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.DocumentTemplates;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces;

public interface IVoucherReservaService
{
    Task<VoucherDocumentResultModel> GerarVoucherAsync(int agendamentoId, bool isTimeSharing);
    Task<VoucherDocumentResultModel> GerarVoucherAsync(long reservaCmIdOuAgendamentIdMultipropriedade,
        bool isTimeSharing,
        List<Models.DadosContratoModel>? contratos,
        AutomaticCommunicationConfigModel config);
    IReadOnlyCollection<PlaceholderDescriptionReservas> ListarPlaceholders();
    Task<DadosImpressaoVoucherResultModel> ObterDadosReservaAsync(long reservaId, bool isTimeSharing);
}

