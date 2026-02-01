using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.TimeSharing;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    /// <summary>
    /// Interface para serviço de reservas TimeSharing com suporte a transações distribuídas
    /// </summary>
    public interface ITimeSharingReservaService
    {
        /// <summary>
        /// Cria uma nova reserva
        /// </summary>
        /// <param name="model">Dados da reserva</param>
        /// <param name="usarSaga">Se true, usa Saga Pattern; se false, usa método tradicional</param>
        /// <returns>Resultado com ID da reserva criada</returns>
        Task<ResultModel<long>> CriarReservaAsync(InclusaoReservaInputModel model, bool usarSaga = true);

        /// <summary>
        /// Cancela uma reserva existente
        /// </summary>
        /// <param name="model">Dados do cancelamento</param>
        /// <param name="usarSaga">Se true, usa Saga Pattern</param>
        /// <returns>Resultado indicando sucesso ou falha</returns>
        Task<ResultModel<bool>> CancelarReservaAsync(CancelarReservaTsModel model, bool usarSaga = true);

        /// <summary>
        /// Altera uma reserva existente
        /// </summary>
        /// <param name="model">Novos dados da reserva</param>
        /// <param name="usarSaga">Se true, usa Saga Pattern</param>
        /// <returns>Resultado com ID da reserva alterada</returns>
        Task<ResultModel<long?>> AlterarReservaAsync(InclusaoReservaInputModel model, bool usarSaga = true);
    }
}
