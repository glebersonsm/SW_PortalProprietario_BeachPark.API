using EsolutionPortalDomain.Enums;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class ReservaRciModel
    {
        public int? Id { get; set; }
        public int? idReservasRci { get; set; }
        public int? IdReservasFront { get; set; }
        public int? IdReservaMigrada { get; set; }
        public string? FlgBulk { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }
        public string? NumeroReserva { get; set; }
        public string? NomeCliente { get; set; }
        public string? NumeroContrato { get; set; }
        public DateTime? DataCriacao { get; set; }
        public DateTime? DataAlteracao { get; set; }
        public DateTime? DataHoraCriacao { get; set; }
        public string? UsuarioAlteracao { get; set; }
        public string? StatusCM { get; set; }
        public string? Hotel { get; set; }
        public DateTime? Checkin { get; set; }
        public DateTime? Checkout { get; set; }
        public string? IdRCI { get; set; }
        public int? ClienteReservante { get; set; }
        public int? ClienteNotificadoCancelamento { get; set; }
        public string? EmailCliente { get; set; }
    }
}

