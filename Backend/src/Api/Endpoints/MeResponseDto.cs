namespace KPG.Timesheet.Api.Endpoints;

public record MeResponseDto(
    string UserId,
    string Email,
    IReadOnlyList<string> Roles,
    string? NombreCompleto,
    string? CodigoPais,
    bool NacionalidadDemostrativa);
