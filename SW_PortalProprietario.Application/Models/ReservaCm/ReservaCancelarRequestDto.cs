namespace SW_PortalProprietario.Application.Models.ReservaCm;

public record ReservaCancelarRequestDto(
    long IdReseva,
    string? MotivoCancelamento,
    string? ObservaoCancelamento
);
