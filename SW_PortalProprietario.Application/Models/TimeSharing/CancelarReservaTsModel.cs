using NHibernate.Mapping;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class CancelarReservaTsModel
    {
        public Int64? ReservaId { get; set; }
        public string? MotivoCancelamento { get; set; } = "1";
        public string? ObservacaoCancelamento { get; set; } = "Cancelamento automático - Portal MVC - SWSoluções";
        public int? ReservaTimesharingId { get; set; }
        public string? MotivoCancelamentoInfUsu { get; set; }
        public bool? NotificarCliente { get; set; }

    }
}
