using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models.ReservaCm;
using SW_PortalProprietario.Application.Services.ReservaCm;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.ReservaCm;

[ApiController]
[Route("ReservaCM/v1")]
[Route("Reserva/v1")] // Rota original do SW_CMApi - compatibilidade para remoção do projeto
public class ReservaCMController : ControllerBase
{
    private readonly IReservaCMService _reservaService;

    public ReservaCMController(IReservaCMService reservaService)
    {
        _reservaService = reservaService;
    }

    [HttpPost("efetuarReserva"), Authorize(Roles = "Administrador, Usuario")]
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

    [HttpPost("cancelarReserva"), Authorize(Roles = "Administrador, Usuario")]
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
