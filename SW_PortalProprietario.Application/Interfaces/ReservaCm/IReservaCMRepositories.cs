using CMDomain.Entities.ReservaCm;
using System.Threading.Tasks;

namespace SW_PortalProprietario.Application.Interfaces.ReservaCm;

public interface IParametroHotelCMRepository
{
    Task<ParametroHotelCm?> GetByIdHotelAsync(long idHotel);
}
