namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record VentanaRetroactividadResponse(int Ventana);

public record UmbralNotificacionResponse(int Dias);

public record UpdateVentanaRequest(int Dias);

public record UpdateUmbralRequest(int Dias);
