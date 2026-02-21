namespace SW_PortalProprietario.Application.Models.ReservaCm;

public record ApiResponseDto<T>(
    int Status,
    bool Success,
    T? Data,
    string Message
);
