using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models.ReservaCm;
using SW_PortalProprietario.Application.Services.ReservaCm;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.ReservaCm;

[ApiController]
[Route("ReservaCM/v1")]
public class ReservaCMController : ControllerBase
{
    private readonly IReservaCMService _reservaService;

    public ReservaCMController(IReservaCMService reservaService)
    {
        _reservaService = reservaService;
    }

    [HttpPost("efetuarReserva")]
    public async Task<IActionResult> CriarReserva([FromBody] ReservaRequestDto reservaDto)
    {
        try
        {
            var dadosDaReserva = await _reservaService.SalvarReservaAsync(reservaDto);
            var respostaApi = new ApiResponseDto<ReservaResponseDataDto>(201, true, dadosDaReserva, "Reserva criada com sucesso.");
            return Created("", respostaApi);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponseDto<object?>(400, false, null, ex.Message));
        }
    }

    [HttpPost("cancelarReserva")]
    public async Task<IActionResult> CancelarReserva([FromBody] ReservaCancelarRequestDto reservaCancelar)
    {
        try
        {
            var mensagem = await _reservaService.CancelarReservaAsync(reservaCancelar);
            var respostaApi = new ApiResponseDto<object?>(200, true, null, mensagem);
            return Ok(respostaApi);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponseDto<object?>(400, false, null, ex.Message));
        }
    }
}
