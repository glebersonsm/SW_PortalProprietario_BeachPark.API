using CMDomain.Entities.ReservaCm;
using System.Threading.Tasks;

namespace SW_PortalProprietario.Application.Interfaces.ReservaCm;

public interface IReservaCMRepository
{
    Task<ReservaFront?> GetByIdAsync(long id);
    Task<ReservaFront?> GetByNumeroReservaAsync(long numeroReserva);
    Task AddAsync(ReservaFront reserva);
    Task UpdateAsync(ReservaFront reserva);
    Task SaveChangesAsync();
}
