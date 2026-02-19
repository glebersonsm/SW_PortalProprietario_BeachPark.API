using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface IIncentivoParaAgendamentoService
    {
        Task<List<EmailInputInternalModel>> GenerateSimulationEmailAsync(
        AutomaticCommunicationConfigModel config,
        string userEmail,
        int userId);

        Task<IncentivoParaAgendamentoEmailDataModel?> GerarAvisoCompletoAsync(
        AutomaticCommunicationConfigModel config,
        DadosContratoModel contrato,
        PosicaoAgendamentoViewModel statusAgendamento,
        int daysBefore);


    }
}