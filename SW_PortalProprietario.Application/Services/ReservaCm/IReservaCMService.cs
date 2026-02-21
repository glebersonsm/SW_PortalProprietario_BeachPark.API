using SW_PortalProprietario.Application.Models.ReservaCm;
using System.Threading.Tasks;

namespace SW_PortalProprietario.Application.Services.ReservaCm;

public interface IReservaCMService
{
    Task<ReservaResponseDataDto> SalvarReservaAsync(ReservaRequestDto reservaDto);
    Task<string> CancelarReservaAsync(ReservaCancelarRequestDto reservaCancelar);
}
