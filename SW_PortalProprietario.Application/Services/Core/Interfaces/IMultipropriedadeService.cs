using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    /// <summary>
    /// Interface para serviço de Multipropriedade com suporte a transações distribuídas
    /// </summary>
    public interface IMultipropriedadeService
    {
        /// <summary>
        /// Salva uma reserva em agendamento
        /// </summary>
        /// <param name="model">Dados da reserva</param>
        /// <param name="usarSaga">Se true, usa Saga Pattern</param>
        Task<ResultModel<int>> SalvarReservaEmAgendamentoAsync(
            CriacaoReservaAgendamentoInputModel model, 
            bool usarSaga = true);

        /// <summary>
        /// Libera semana para pool
        /// </summary>
        /// <param name="model">Dados da liberação</param>
        /// <param name="usarSaga">Se true, usa Saga Pattern</param>
        Task<ResultModel<bool>> LiberarMinhaSemanaPoolAsync(
            LiberacaoMeuAgendamentoInputModel model, 
            bool usarSaga = true);

        /// <summary>
        /// Troca de semana
        /// </summary>
        /// <param name="model">Dados da troca</param>
        /// <param name="usarSaga">Se true, usa Saga Pattern</param>
        Task<ResultModel<int>> TrocarSemanaAsync(
            TrocaSemanaInputModel model, 
            bool usarSaga = true);
    }
}
