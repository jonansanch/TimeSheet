namespace KPG.Timesheet.Application.Common.Exceptions;

/// <summary>
/// Una capacidad opcional no esta configurada en este ambiente (por ejemplo, la
/// interpretacion de voz por IA sin API key). Se traduce a 503 para que el cliente
/// distinga "no esta disponible" de "fallo": ante un 503 puede caer a su alternativa
/// local en vez de mostrar un error.
/// </summary>
public class ServicioNoDisponibleException(string message) : Exception(message);
