using NHibernate.SqlCommand;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class ReservaGeralTsModel
    {
        public string? Confidencial { get; set; }
        public int? IdReservasFront { get; set; }
        public string? TipoUh { get; set; }
        public int? NumReserva { get; set; }
        public DateTime? DataReserva { get; set; }
        public DateTime? Checkin { get; set; }
        public DateTime? Checkout { get; set; }
        public string? TipoHospede { get; set; }
        public string? NomeHospede { get; set; }
        public string? Segmento { get; set; }
        public string? Origem { get; set; }
        public int? Adultos { get; set; }
        public int? Criancas1 { get; set; }
        public int? Criancas2 { get; set; }
        public string? NumeroContrato { get; set; }
        public string? ProjetoXContrato { get; set; }
        public string? ProjectXContract => ProjetoXContrato;
        public int? IdPessoa { get; set; }
        public string? NomeCliente { get; set; }
        public string? Hotel { get; set; }
        public string? NumDocumentoCliente { get; set; }
        public string? EmailCliente { get; set; }
        public string? ContratoCancelado { get; set; }
        public string? StatusReserva { get; set; }
        public DateTime? DataCancelamento { get; set; }
        public string? Observacoes { get; set; }
        public string? Tarifa { get; set; }
        public string? ClienteHotel { get; set; }
        public string? RazaoSocialClienteHotel { get; set; }
        public string? Rci { get; set; }
        public decimal? PontosDebitados { get; set; }
        public string? TipoReserva { get; set; }
        public string? IdRCI { get; set; }
        public int? IdHotel { get; set; }
        public string? TipoDeUso { get; set; }

    }
}
