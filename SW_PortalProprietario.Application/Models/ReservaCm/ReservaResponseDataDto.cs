using System;
using System.Collections.Generic;

namespace SW_PortalProprietario.Application.Models.ReservaCm;

public record ReservaResponseDataDto(
    long? Id,
    long? IdReservasFront,
    long? NumReserva,
    DateTime DataReserva,
    DateTime Checkin,
    DateTime Checkout,
    string? TipoSemana,
    string? Status,
    string? TipoPensao,
    string? TipoHospede,
    string? TipoUhNome,
    int? Adultos,
    int? Criancas1,
    int? Criancas2,
    string? HotelNome,
    string? NomeHospede,
    long? PeriodoCotaDisponibilidadeId,
    string? Cota,
    string? ProprietarioNome,
    string? ProprietarioCpfCnpj,
    long? ProprietarioId,
    long? UhCondominioId,
    int? Capacidade,
    string? TipoUtilizacao,
    string? TipoUso,
    string? TipoDisponibilizacao,
    string? TipoDisponibilizacaoNome,
    List<HospedeResponseDto>? Hospedes
);
