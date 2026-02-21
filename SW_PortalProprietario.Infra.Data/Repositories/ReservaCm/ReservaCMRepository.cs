using CMDomain.Entities.ReservaCm;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.ReservaCm;
using System.Linq;
using System.Threading.Tasks;

namespace SW_PortalProprietario.Infra.Data.Repositories.ReservaCm;

public class ReservaCMRepository : IReservaCMRepository
{
    private readonly IUnitOfWorkNHCm _unitOfWork;

    public ReservaCMRepository(IUnitOfWorkNHCm unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task AddAsync(ReservaFront reserva)
    {
        ArgumentNullException.ThrowIfNull(_unitOfWork.Session, nameof(_unitOfWork.Session));
        await _unitOfWork.Session.InsertAsync(reserva, _unitOfWork.CancellationToken);
    }

    public async Task<ReservaFront?> GetByIdAsync(long id)
    {
        ArgumentNullException.ThrowIfNull(_unitOfWork.Session, nameof(_unitOfWork.Session));
        return await _unitOfWork.Session.GetAsync<ReservaFront>(id, _unitOfWork.CancellationToken);
    }

    public async Task<ReservaFront?> GetByNumeroReservaAsync(long numeroReserva)
    {
        ArgumentNullException.ThrowIfNull(_unitOfWork.Session, nameof(_unitOfWork.Session));
        var query = _unitOfWork.Session.CreateQuery("from ReservaFront r where r.NumeroReserva = :numeroReserva");
        query.SetParameter("numeroReserva", numeroReserva);
        var list = await query.ListAsync<ReservaFront>(_unitOfWork.CancellationToken);
        return list.FirstOrDefault();
    }

    public async Task SaveChangesAsync()
    {
        _unitOfWork.Session?.GetSessionImplementation()?.Flush();
        await Task.CompletedTask;
    }

    public async Task UpdateAsync(ReservaFront reserva)
    {
        ArgumentNullException.ThrowIfNull(_unitOfWork.Session, nameof(_unitOfWork.Session));
        await _unitOfWork.Session.UpdateAsync(reserva, _unitOfWork.CancellationToken);
    }
}
