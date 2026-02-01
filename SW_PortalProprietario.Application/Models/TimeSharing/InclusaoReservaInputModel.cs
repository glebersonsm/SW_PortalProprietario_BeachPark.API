using NHibernate.Mapping;
using System.Linq;
using SW_PortalProprietario.Application.Models;
using Dapper;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class InclusaoReservaInputModel
    {
        public int? IdHotel { get; set; }
        public int? TipoUhEstadia { get; set; }
        public int? TipoUhTarifa { get; set; }
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
        public int? TipoTarifa { get; set; }
        public DateTime? DataHora { get; set; }
        public Int64? Reserva { get; set; }
        public Int64? NumReserva { get; set; }
        public Int64? IdReservasFront { get; set; }
        public string? NumeroContrato { get; set; }
        public int? IdVenda { get; set; }
        public int? IdVendaXContrato { get; set; }
        public DateTime? Checkin { get; set; }
        public DateTime? Checkout { get; set; }
        public int? QuantidadeAdultos { get; set; }
        public int? QuantidadeCrianca1 { get; set; }
        public int? QuantidadeCrianca2 { get; set; }
        public int? IdPessoaChave { get; set; }
        public int? IdFracionamentoTs { get; set; }
        public decimal? NumeroPontos { get; set; }
        public Int64? Id { get; set; }
        public List<HospedeInputModel> Hospedes { get; set; } = new List<HospedeInputModel>();
        public int QtdePaxConsiderar { get; set; }
        public int QtdePessoasFree { get; set; }
        public string? TipoDeBusca { get; set; }
        public string? LoginPms { get; set; }
        public string? LoginSistemaVenda { get; set; }

        public static explicit operator InclusaoReservaInputDto(InclusaoReservaInputModel model)
        {
            return new InclusaoReservaInputDto
            {
                IdHotel = model.IdHotel,
                TipoUhTarifa = model.TipoUhTarifa,
                TipoUhEstadia = model.TipoUhEstadia,
                IdTipoUh = model.IdTipoUh,
                Segmento = model.Segmento,
                Origem = model.Origem,
                MeioComunicacao = model.MeioComunicacao,
                IdVeiculo = model.IdVeiculo,
                Status = model.Status,
                ClienteReservante = model.ClienteReservante,
                TipoUtilizacao = model.TipoUtilizacao,
                TipoUso = model.TipoUso,
                AgendamentoId = model.AgendamentoId,
                InventarioId = model.InventarioId,
                LocReserva = model.LocReserva,
                Observacao = model.Observacao,
                CodigoPensao = model.CodigoPensao,
                GaranteNoShow = model.GaranteNoShow,
                CodigoUh = model.CodigoUh,
                MotivoDesconto = model.MotivoDesconto,
                TipoTarifacao = model.TipoTarifa,
                Reserva = model.Reserva,
                NumeroContrato = model.NumeroContrato,
                IdVenda = model.IdVenda,
                IdVendaXContrato = model.IdVendaXContrato,
                CheckIn = model.Checkin?.ToString("yyyy-MM-dd"),
                CheckOut = model.Checkout?.ToString("yyyy-MM-dd"),
                QuantidadeAdultos = model.QuantidadeAdultos,
                QuantidadeCrianca1 = model.QuantidadeCrianca1,
                QuantidadeCrianca2 = model.QuantidadeCrianca2,
                IdPessoaChave = model.IdPessoaChave,
                IdFracionamentoTs = model.IdFracionamentoTs,
                NumeroPontos = model.NumeroPontos,
                Hospedes = model.Hospedes.Any() ? model.Hospedes.Select(h => (HospedeInputDto)h).AsList() : new List<HospedeInputDto>(),
                Id = model.Id,
                IdReservasFront = model.IdReservasFront,
                NumReserva = model.NumReserva,
                LoginPms = model.LoginPms,
                LoginSistemaVenda = model.LoginSistemaVenda
            };
        }

    }
}
