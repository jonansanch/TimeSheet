using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Interfaces;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;
using ApplicationValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;

public class CreateRegistroHorasCommandHandler : IRequestHandler<CreateRegistroHorasCommand, RegistroHorasDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateRegistroHorasCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<RegistroHorasDto> Handle(CreateRegistroHorasCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("No existe usuario autenticado para asociar el registro.");

        var duplicateExists = await _context.RegistrosHoras
            .AnyAsync(r =>
                r.UserId == userId &&
                r.FechaRegistro == request.FechaRegistro &&
                r.Turno == request.Turno,
                cancellationToken);

        if (duplicateExists)
        {
            throw new ApplicationValidationException([
                new ValidationFailure(
                    nameof(request.Turno),
                    "Ya existe un registro para este usuario, fecha y turno.")
            ]);
        }

        var registro = new RegistroHorasEntity(
            userId,
            request.FechaRegistro,
            request.Turno,
            request.HoraEntrada,
            request.HoraSalida,
            request.Cliente,
            request.Proyecto,
            request.Modalidad,
            request.Recurso,
            request.Descripcion,
            request.Lugar);

        _context.RegistrosHoras.Add(registro);
        await _context.SaveChangesAsync(cancellationToken);

        return new RegistroHorasDto(
            registro.Id,
            registro.UserId,
            registro.FechaRegistro,
            registro.Turno,
            registro.HoraEntrada,
            registro.HoraSalida,
            registro.Cliente,
            registro.Proyecto,
            registro.Modalidad,
            registro.Recurso,
            registro.Descripcion,
            registro.Lugar);
    }
}
