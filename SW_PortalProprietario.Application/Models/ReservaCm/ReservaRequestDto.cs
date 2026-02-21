using System;
using System.Collections.Generic;

namespace SW_PortalProprietario.Application.Models.ReservaCm;

public record ReservaRequestDto(
    long? Id,
    long? Reserva,
    long? NumReserva,
    long? IdReservasFront,
    string IdHotel,
    int? TipoUhTarifa,
    int? TipoUhEstadia,
    int? IdTipoUh,
    string? Segmento,
    string? Origem,
    string? MeioComunicacao,
    string? IdVeiculo,
    string? Status,
    string? ClienteReservante,
    int TipoTarifa,
    DateTime CheckIn,
    DateTime CheckOut,
    DateTime? DataHora,
    int? QuantidadeAdultos,
    int? QuantidadeCrianca1,
    int? QuantidadeCrianca2,
    string? TipoUtilizacao,
    string? TipoUso,
    long AgendamentoId,
    long? InventarioId,
    string? LocReserva,
    string? Observacao,
    string? CodigoPensao,
    string? GaranteNoShow,
    string? CodigoUh,
    string? MotivoDesconto,
    List<HospedeDto> Hospedes,
    string? LoginPms,
    string? LoginSistemaVenda
);
