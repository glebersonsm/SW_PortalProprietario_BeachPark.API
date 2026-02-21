using CMDomain.Entities.ReservaCm;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.ReservaCm;
using System.Threading.Tasks;

namespace SW_PortalProprietario.Infra.Data.Repositories.ReservaCm;

public class ParametroHotelCMRepository : IParametroHotelCMRepository
{
    private readonly IUnitOfWorkNHCm _unitOfWork;

    public ParametroHotelCMRepository(IUnitOfWorkNHCm unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ParametroHotelCm?> GetByIdHotelAsync(long idHotel)
    {
        ArgumentNullException.ThrowIfNull(_unitOfWork.Session, nameof(_unitOfWork.Session));
        return await _unitOfWork.Session.GetAsync<ParametroHotelCm>(idHotel, _unitOfWork.CancellationToken);
    }
}
