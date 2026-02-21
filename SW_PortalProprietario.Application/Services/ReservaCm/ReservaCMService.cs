using CMDomain.Entities.ReservaCm;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.ReservaCm;
using SW_PortalProprietario.Application.Models.ReservaCm;
using SW_PortalProprietario.Application.Services.ReservaCm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SW_PortalProprietario.Application.Services.ReservaCm;

public class ReservaCMService : IReservaCMService
{
    private readonly IReservaCMRepository _reservaRepository;
    private readonly IParametroHotelCMRepository _parametroHotelRepository;
    private readonly IUnitOfWorkNHCm _unitOfWork;

    public ReservaCMService(
        IReservaCMRepository reservaRepository,
        IParametroHotelCMRepository parametroHotelRepository,
        IUnitOfWorkNHCm unitOfWork)
    {
        _reservaRepository = reservaRepository;
        _parametroHotelRepository = parametroHotelRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReservaResponseDataDto> SalvarReservaAsync(ReservaRequestDto reservaDto)
    {
        ValidarReserva(reservaDto);

        long idHotel = long.Parse(reservaDto.IdHotel);
        var parametroHotel = await _parametroHotelRepository.GetByIdHotelAsync(idHotel)
            ?? throw new InvalidOperationException($"Parâmetros do hotel {idHotel} não encontrados.");

        var hospedePrincipalDto = ExtrairHospedePrincipal(reservaDto.Hospedes ?? new List<HospedeDto>());

        var reserva = await BuscarOuCriarReserva(reservaDto, parametroHotel);

        if (long.TryParse(reservaDto.ClienteReservante, out long idCliente))
        {
            reserva.IdCliente = idCliente;
        }

        _unitOfWork.BeginTransaction();
        try
        {
            if (reserva.IdReserva == 0)
            {
                await _reservaRepository.AddAsync(reserva);
            }
            else
            {
                await _reservaRepository.UpdateAsync(reserva);
            }

            await CriarReservaReduzida(reserva);
            var hospedesResponse = await CriarHospedesReserva(reserva, reservaDto.Hospedes ?? new List<HospedeDto>(), parametroHotel);

            var (executed, exception) = await _unitOfWork.CommitAsync();
            if (!executed && exception != null)
                throw exception;

            return MapToResponse(reserva, hospedesResponse);
        }
        catch
        {
            _unitOfWork.Rollback();
            throw;
        }
    }

    private static HospedeDto ExtrairHospedePrincipal(List<HospedeDto> hospedes)
    {
        var principais = hospedes.Where(h => "S".Equals(h.Principal, StringComparison.OrdinalIgnoreCase)).ToList();
        if (principais.Count != 1)
            throw new Exception($"A reserva deve ter exatamente um hóspede principal. Encontrados: {principais.Count}");
        return principais.First();
    }

    private async Task<ReservaFront> BuscarOuCriarReserva(ReservaRequestDto dto, ParametroHotelCm parametroHotel)
    {
        ReservaFront? reserva = null;

        if (dto.IdReservasFront > 0)
        {
            reserva = await _reservaRepository.GetByIdAsync(dto.IdReservasFront.Value);
        }
        else if (dto.NumReserva > 0)
        {
            reserva = await _reservaRepository.GetByNumeroReservaAsync(dto.NumReserva.Value);
        }

        if (reserva != null)
        {
            ValidarAlteracaoReserva(reserva, dto);
            AtualizarReservaFromDto(reserva, dto, parametroHotel);
            return reserva;
        }

        reserva = new ReservaFront();
        AtualizarReservaFromDto(reserva, dto, parametroHotel);
        return reserva;
    }

    private static void AtualizarReservaFromDto(ReservaFront reserva, ReservaRequestDto dto, ParametroHotelCm parametroHotel)
    {
        reserva.IdHotel = long.Parse(dto.IdHotel);
        reserva.DataCheckinPrevisto = dto.CheckIn;
        reserva.DataCheckoutPrevisto = dto.CheckOut;
        reserva.QuantidadeAdulto = dto.QuantidadeAdultos;
        reserva.QuantidadeCrianca1 = dto.QuantidadeCrianca1;
        reserva.QuantidadeCrianca2 = dto.QuantidadeCrianca2;
        reserva.NumeroReserva = dto.NumReserva ?? (reserva.NumeroReserva == 0 ? 0 : reserva.NumeroReserva);
        reserva.Observacao = dto.Observacao;

        if (reserva.IdReserva == 0)
        {
            reserva.StatusReserva = (long)StatusReservaCm.CONFIRMAR;
            reserva.DataReserva = DateTime.Now;
        }
    }

    private static void ValidarAlteracaoReserva(ReservaFront reserva, ReservaRequestDto dto)
    {
        if (reserva.StatusReserva == (long)StatusReservaCm.CHECKIN)
            throw new Exception("Reserva em checkin não pode ser alterada");
        if (reserva.StatusReserva == (long)StatusReservaCm.CANCELADA)
            throw new Exception("Reserva cancelada não pode ser alterada");
    }

    private async Task CriarReservaReduzida(ReservaFront reserva)
    {
        var reduzida = new ReservaReduzidaCm
        {
            IdReserva = reserva.IdReserva,
            IdHotel = reserva.IdHotel,
            DataCheckinPrevisto = reserva.DataCheckinPrevisto,
            DataCheckoutPrevisto = reserva.DataCheckoutPrevisto,
            StatusReserva = reserva.StatusReserva,
            TipoUh = reserva.IdTipoUhTarifa,
            UsuarioInclusao = "SYSTEM"
        };
        ArgumentNullException.ThrowIfNull(_unitOfWork.Session, nameof(_unitOfWork.Session));
        await _unitOfWork.Session.InsertAsync(reduzida, _unitOfWork.CancellationToken);
    }

    private async Task<List<HospedeResponseDto>> CriarHospedesReserva(ReservaFront reserva, List<HospedeDto> hospedes, ParametroHotelCm parametroHotel)
    {
        var responseList = new List<HospedeResponseDto>();
        ArgumentNullException.ThrowIfNull(_unitOfWork.Session, nameof(_unitOfWork.Session));

        foreach (var hospDto in hospedes)
        {
            var hospede = new HospedeCm
            {
                Nome = hospDto.Nome,
                DataNascimento = hospDto.DataNascimento,
                TipoEtario = 1
            };
            if (hospDto.Id > 0)
                hospede.IdHospede = hospDto.Id;

            await _unitOfWork.Session.InsertAsync(hospede, _unitOfWork.CancellationToken);

            var mov = new MovimentoHospedeCm
            {
                IdResevasFront = reserva.IdReserva,
                IdHospede = hospede.IdHospede,
                IdHotel = reserva.IdHotel,
                DataChekinPrevisto = reserva.DataCheckinPrevisto,
                DataCheckoutPrevisto = reserva.DataCheckoutPrevisto,
                Principal = hospDto.Principal ?? "N",
                UsuarioInclusao = "SYSTEM"
            };
            await _unitOfWork.Session.InsertAsync(mov, _unitOfWork.CancellationToken);

            responseList.Add(new HospedeResponseDto(hospede.IdHospede, hospede.IdHospede));
        }

        return responseList;
    }

    private static void ValidarReserva(ReservaRequestDto reservaDto)
    {
        if (reservaDto.CheckIn.Date < DateTime.Today)
            throw new Exception("Data de Checkin deve ser maior ou igual à data atual");
    }

    private static ReservaResponseDataDto MapToResponse(ReservaFront reserva, List<HospedeResponseDto> hospedes)
    {
        return new ReservaResponseDataDto(
            reserva.IdReserva,
            reserva.IdReserva,
            reserva.NumeroReserva,
            reserva.DataReserva ?? DateTime.MinValue,
            reserva.DataCheckinPrevisto ?? DateTime.MinValue,
            reserva.DataCheckoutPrevisto ?? DateTime.MinValue,
            null,
            reserva.StatusReserva?.ToString(),
            null, null, null,
            (int?)reserva.QuantidadeAdulto,
            (int?)reserva.QuantidadeCrianca1,
            (int?)reserva.QuantidadeCrianca2,
            null, null, null, null, null, null, null, null, null, null, null, null, null,
            hospedes
        );
    }

    public async Task<string> CancelarReservaAsync(ReservaCancelarRequestDto reservaCancelar)
    {
        if (reservaCancelar.IdReseva == 0)
        {
            throw new ArgumentException("Não informado o número da reserva a cancelar");
        }

        var reserva = await _reservaRepository.GetByNumeroReservaAsync(reservaCancelar.IdReseva);
        if (reserva == null)
        {
            throw new KeyNotFoundException($"Reserva com número {reservaCancelar.IdReseva} não encontrada!");
        }

        long idMotivo = 0;
        long.TryParse(reservaCancelar.MotivoCancelamento, out idMotivo);

        reserva.Cancelar(idMotivo, reservaCancelar.ObservaoCancelamento ?? string.Empty);

        _unitOfWork.BeginTransaction();
        try
        {
            await _reservaRepository.UpdateAsync(reserva);
            await _reservaRepository.SaveChangesAsync();

            var (executed, exception) = await _unitOfWork.CommitAsync();
            if (!executed && exception != null)
                throw exception;

            return "Reserva cancelada com sucesso!";
        }
        catch
        {
            _unitOfWork.Rollback();
            throw;
        }
    }
}
