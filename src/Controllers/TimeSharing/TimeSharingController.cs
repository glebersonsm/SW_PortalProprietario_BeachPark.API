using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;

namespace SW_PortalProprietario.API.src.Controllers.TimeSharing
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class TimeSharingController : ControllerBase
    {

        private readonly ITimeSharingProviderService _timeSharingProviderService;

        public TimeSharingController(ITimeSharingProviderService timeSharingService)
        {
            _timeSharingProviderService = timeSharingService;
        }


        [HttpGet("contratosTimeSharing"), Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ContratoTimeSharingModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ContratoTimeSharingModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ContratoTimeSharingModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProprietariosContratosTimeSharing([FromQuery] SearchContratosTimeSharingModel searchModel)
        {
            try
            {
                var result = await _timeSharingProviderService.GetContratosTimeSharing(searchModel);
                if (result == null || !result.Value.contratos.Any())
                    return Ok(new ResultWithPaginationModel<List<ContratoTimeSharingModel>>()
                    {
                        Data = new List<ContratoTimeSharingModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        LastPageNumber = -1,
                        PageNumber = -1,
                        Success = true
                    });
                else return Ok(new ResultWithPaginationModel<List<ContratoTimeSharingModel>>(result.Value.contratos.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    LastPageNumber = result.Value.lastPageNumber,
                    PageNumber = result.Value.pageNumber,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultWithPaginationModel<List<ContratoTimeSharingModel>>()
                {
                    Data = new List<ContratoTimeSharingModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<ContratoTimeSharingModel>>()
                {
                    Data = new List<ContratoTimeSharingModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
        }

        [HttpGet("searchReservasComPontosBaixados"), Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaTsModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaTsModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaTsModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchReservasGeralComConsumoPontos([FromQuery] SearchReservaTsModel searchModel)
        {
            try
            {
                var result = await _timeSharingProviderService.GetReservasGeralComConsumoPontos(searchModel);
                if (result == null || !result.Value.reservas.Any())
                    return Ok(new ResultWithPaginationModel<List<ReservaTsModel>>()
                    {
                        Data = new List<ReservaTsModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        LastPageNumber = -1,
                        PageNumber = -1,
                        Success = true
                    });
                else return Ok(new ResultWithPaginationModel<List<ReservaTsModel>>(result.Value.reservas.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    LastPageNumber = result.Value.lastPageNumber,
                    PageNumber = result.Value.pageNumber,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultWithPaginationModel<List<ReservaTsModel>>()
                {
                    Data = new List<ReservaTsModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<ReservaTsModel>>()
                {
                    Data = new List<ReservaTsModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
        }

        [HttpGet("searchReservas"), Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaGeralTsModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaGeralTsModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaGeralTsModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchReservas([FromQuery] SearchReservasGeralModel searchModel)
        {
            try
            {
                var result = await _timeSharingProviderService.GetReservasGeral(searchModel);
                if (result == null || !result.Value.reservas.Any())
                    return Ok(new ResultWithPaginationModel<List<ReservaGeralTsModel>>()
                    {
                        Data = new List<ReservaGeralTsModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        LastPageNumber = -1,
                        PageNumber = -1,
                        Success = true
                    });
                else return Ok(new ResultWithPaginationModel<List<ReservaGeralTsModel>>(result.Value.reservas.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    LastPageNumber = result.Value.lastPageNumber,
                    PageNumber = result.Value.pageNumber,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultWithPaginationModel<List<ReservaGeralTsModel>>()
                {
                    Data = new List<ReservaGeralTsModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<ReservaGeralTsModel>>()
                {
                    Data = new List<ReservaGeralTsModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
        }

        [HttpGet("searchReservasRci"), Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaRciModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaRciModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaRciModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchReservasRci([FromQuery] SearchReservasRciModel searchModel)
        {
            try
            {
                var result = await _timeSharingProviderService.GetReservasRci(searchModel);
                if (result == null || !result.Value.reservas.Any())
                    return Ok(new ResultWithPaginationModel<List<ReservaRciModel>>()
                    {
                        Data = new List<ReservaRciModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        LastPageNumber = -1,
                        PageNumber = -1,
                        Success = true
                    });
                else return Ok(new ResultWithPaginationModel<List<ReservaRciModel>>(result.Value.reservas.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    LastPageNumber = result.Value.lastPageNumber,
                    PageNumber = result.Value.pageNumber,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultWithPaginationModel<List<ReservaRciModel>>()
                {
                    Data = new List<ReservaRciModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<ReservaRciModel>>()
                {
                    Data = new List<ReservaRciModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
        }

        [HttpGet("hoteisVinculados"), Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<HotelModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<HotelModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<HotelModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHoteisVinculados()
        {
            try
            {
                var result = await _timeSharingProviderService.HoteisVinculados();
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<HotelModel>>()
                    {
                        Data = new List<HotelModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<HotelModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<HotelModel>>()
                {
                    Data = new List<HotelModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<HotelModel>>()
                {
                    Data = new List<HotelModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("vincularReservaRCI"), Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VincularReservaRCI([FromBody] VincularReservaRciModel vincularModel)
        {
            try
            {
                var result = await _timeSharingProviderService.VincularReservaRCI(vincularModel);
                if (result)
                    return Ok(new ResultModel<bool>()
                    {
                        Data = true,
                        Errors = new List<string>(),
                        Status = StatusCodes.Status200OK,
                        Success = true
                    });
                else return Ok(new ResultModel<bool>()
                {
                    Data = false,
                    Errors = new List<string>() { "Ops! Nenhum registro foi alterado!" },
                    Status = StatusCodes.Status404NotFound,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<bool>()
                {
                    Data = false,
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível vincular a reserva RCI", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível vincular a reserva RCI", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<bool>()
                {
                    Data = false,
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível vincular a reserva RCI", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível vincular a reserva RCI", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        /// <summary>
        /// Calcula os pontos necessários para uma reserva de forma simplificada
        /// Este endpoint pode ser usado em tempo real pelo frontend para cálculos instantâneos
        /// </summary>
        /// <param name="request">Modelo com dados da reserva (datas, quantidade de pessoas, contrato)</param>
        /// <returns>Pontos necessários e informações complementares</returns>
        [HttpPost("calcularPontos"), Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultModel<CalcularPontosResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<CalcularPontosResponseModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<CalcularPontosResponseModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CalcularPontosNecessarios([FromBody] CalcularPontosRequestModel request)
        {
            try
            {
                // Validações básicas
                if (request == null)
                    return BadRequest(new ResultModel<CalcularPontosResponseModel>
                    {
                        Data = null,
                        Errors = new List<string> { "Request inválido" },
                        Status = StatusCodes.Status400BadRequest,
                        Success = false
                    });

                if (request.DataFinal <= request.DataInicial)
                    return BadRequest(new ResultModel<CalcularPontosResponseModel>
                    {
                        Data = null,
                        Errors = new List<string> { "Data final deve ser maior que data inicial" },
                        Status = StatusCodes.Status400BadRequest,
                        Success = false
                    });

                if (request.IdVendaXContrato <= 0 || string.IsNullOrEmpty(request.NumeroContrato))
                    return BadRequest(new ResultModel<CalcularPontosResponseModel>
                    {
                        Data = null,
                        Errors = new List<string> { "Contrato inválido" },
                        Status = StatusCodes.Status400BadRequest,
                        Success = false
                    });

                if (request.HotelId == 0)
                    return BadRequest(new ResultModel<CalcularPontosResponseModel>
                    {
                        Data = null,
                        Errors = new List<string> { "Deve ser informado o Id do Hotel 'HotelId'" },
                        Status = StatusCodes.Status400BadRequest,
                        Success = false
                    });

                if (request.NumReserva.GetValueOrDefault(0) == 0)
                    return BadRequest(new ResultModel<CalcularPontosResponseModel>
                    {
                        Data = null,
                        Errors = new List<string> { "Deve ser informado o número da reserva 'NumReserva'" },
                        Status = StatusCodes.Status400BadRequest,
                        Success = false
                    });

                var response = await _timeSharingProviderService.CalcularPontosNecessarios(request);

                return Ok(new ResultModel<CalcularPontosResponseModel>
                {
                    Data = response,
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<CalcularPontosResponseModel>
                {
                    Data = null,
                    Errors = err.InnerException != null
                        ? new List<string> { "Não foi possível calcular pontos", err.Message, err.InnerException.Message }
                        : new List<string> { "Não foi possível calcular pontos", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<CalcularPontosResponseModel>
                {
                    Data = null,
                    Errors = err.InnerException != null
                        ? new List<string> { "Erro interno ao calcular pontos", err.Message, err.InnerException.Message }
                        : new List<string> { "Erro interno ao calcular pontos", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        
    }
}
