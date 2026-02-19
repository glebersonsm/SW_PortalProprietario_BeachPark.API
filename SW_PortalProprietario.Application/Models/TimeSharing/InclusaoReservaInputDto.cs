using NHibernate.Mapping;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class InclusaoReservaInputDto
    {
        public Int64? Id { get; set; }
        public Int64? NumReserva { get; set; }
        public Int64? IdReservasFront { get; set; }
        public int? IdHotel { get; set; } = 3;
        public int? TipoUhTarifa { get; set; }
        public int? TipoUhEstadia { get; set; }
        public int? IdTipoUh { get; set; }
        public string? Segmento { get; set; }
        public string? Origem { get; set; }
        public string? MeioComunicacao { get; set; }
        public string? IdVeiculo { get; set; }
        public string? Status { get; set; }
        public string? ClienteReservante { get; set; }
        public string? TipoUtilizacao { get; set; }
        public string? TipoUso { get; set; }
        public string? TipoDeUso { get; set; }
        public Int64? AgendamentoId { get; set; }
        public Int64? InventarioId { get; set; }
        public int? LocReserva { get; set; }
        public string? Observacao { get; set; }
        public string? CodigoPensao { get; set; } = "N";
        public string? GaranteNoShow { get; set; } = "N";
        public string? CodigoUh { get; set; }
        public string? MotivoDesconto { get; set; }
        public int? TipoTarifacao { get; set; }
        public string? DataHora { get; set; }
        public Int64? Reserva { get; set; }
        public string? NumeroContrato { get; set; }
        public int? IdVenda { get; set; }
        public int? IdVendaXContrato { get; set; }
        public string? CheckIn { get; set; }
        public string? CheckOut { get; set; }
        public int? QuantidadeAdultos { get; set; }
        public int? QuantidadeCrianca1 { get; set; }
        public int? QuantidadeCrianca2 { get; set; }
        public int? IdPessoaChave { get; set; }
        public int? IdFracionamentoTs { get; set; }
        public decimal? NumeroPontos { get; set; }
        public List<HospedeInputDto> Hospedes { get; set; } = new List<HospedeInputDto>();
        public string? LoginPms { get; set; }
        public string? LoginSistemaVenda { get; set; }

    }
}
