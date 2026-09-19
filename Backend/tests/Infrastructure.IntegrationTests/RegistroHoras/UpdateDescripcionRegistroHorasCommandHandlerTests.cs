using FluentAssertions;
using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.UpdateDescripcionRegistroHoras;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using KPG.Timesheet.Infrastructure.Identity;
using KPG.Timesheet.Infrastructure.Organizacion;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

/// <summary>
/// Editar un registro es potestad de <b>sus</b> supervisores (puesto, jefe directo o
/// responsable del proyecto), no de cualquiera que tenga el rol Supervisor.
/// </summary>
public class UpdateDescripcionRegistroHorasCommandHandlerTests
{
    private const string Empleado  = "user-empleado";
    private const string SupPuesto = "user-sup-puesto";
    private const string Jefe      = "user-jefe";
    private const string SupProy   = "user-sup-proyecto";
    private const string Ajeno     = "user-ajeno";

    [Theory]
    [InlineData(SupPuesto)]
    [InlineData(Jefe)]
    [InlineData(SupProy)]
    public async Task Handle_ConCualquieraDeLosTresSupervisores_ActualizaDescripcion(string actorId)
    {
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);

        await Handler(context, actorId).Handle(
            new UpdateDescripcionRegistroHorasCommand(id, "Descripción actualizada"),
            CancellationToken.None);

        var updated = await context.RegistrosHoras.FindAsync(id);
        updated!.Descripcion.Should().Be("Descripción actualizada");
    }

    [Fact]
    public async Task Handle_ConUnSupervisorAjenoAlRegistro_LanzaForbidden()
    {
        // El hueco que esto cierra: antes bastaba con tener el rol Supervisor.
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);

        var act = () => Handler(context, Ajeno).Handle(
            new UpdateDescripcionRegistroHorasCommand(id, "No deberia poder"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_ConAdmin_ActualizaAunqueNoSeaSuSupervisado()
    {
        // Admin ya puede eliminar registros: bloquearle la edicion seria incoherente.
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);

        await Handler(context, Ajeno, esAdmin: true).Handle(
            new UpdateDescripcionRegistroHorasCommand(id, "Corrección administrativa"),
            CancellationToken.None);

        var updated = await context.RegistrosHoras.FindAsync(id);
        updated!.Descripcion.Should().Be("Corrección administrativa");
    }

    [Fact]
    public async Task Handle_ConMetadatos_ActualizaSoloLosEnviados()
    {
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);

        await Handler(context, Jefe).Handle(
            new UpdateDescripcionRegistroHorasCommand(id, "Desc", Modalidad: "Cliente"),
            CancellationToken.None);

        var updated = await context.RegistrosHoras.FindAsync(id);
        updated!.Modalidad.Should().Be("Cliente");
        updated.Recurso.Should().Be("Consultor");   // no enviado: conserva su valor
        updated.Lugar.Should().Be("Bogota");
    }

    [Fact]
    public async Task Handle_CuandoElRegistroYaEstaAprobado_LanzaValidationException()
    {
        // Editar tras la aprobacion final invalidaria en silencio las tres firmas.
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);
        var registro = await context.RegistrosHoras.FirstAsync();
        registro.Aprobar(1); registro.Aprobar(2); registro.Aprobar(3);
        await context.SaveChangesAsync(CancellationToken.None);

        var act = () => Handler(context, Jefe).Handle(
            new UpdateDescripcionRegistroHorasCommand(id, "Tarde"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenRegistroNoExiste_LanzaNotFoundException()
    {
        await using var context = await EscenarioAsync();

        var act = () => Handler(context, Jefe).Handle(
            new UpdateDescripcionRegistroHorasCommand(999, "Nueva descripción"),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static UpdateDescripcionRegistroHorasCommandHandler Handler(
        ApplicationDbContext context, string actorId, bool esAdmin = false)
    {
        var user = Substitute.For<IUser>();
        user.Id.Returns(actorId);
        user.Roles.Returns(esAdmin ? [Roles.Admin] : [Roles.Supervisor]);

        return new UpdateDescripcionRegistroHorasCommandHandler(
            context, new CadenaAprobacionService(context), Substitute.For<IBitacoraService>(), user);
    }

    private static Task<int> RegistroIdAsync(ApplicationDbContext context) =>
        context.RegistrosHoras.Select(r => r.Id).FirstAsync();

    private static async Task<ApplicationDbContext> EscenarioAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        var cliente = new Cliente("Banco Nacional");
        context.Clientes.Add(cliente);
        var puesto = new Empleado("Consultor");
        context.Empleados.Add(puesto);
        await context.SaveChangesAsync(CancellationToken.None);

        var proyecto = new Proyecto(cliente.Id, "Core Bancario");
        proyecto.AsignarSupervisor(SupProy);
        context.Proyectos.Add(proyecto);
        context.SupervisoresPuesto.Add(new SupervisorPuesto(puesto.Id, SupPuesto));
        context.Users.Add(new ApplicationUser
        {
            Id = Empleado,
            UserName = "empleado@kpg.com",
            Email = "empleado@kpg.com",
            PuestoId = puesto.Id,
            SupervisorUserId = Jefe
        });
        await context.SaveChangesAsync(CancellationToken.None);

        context.RegistrosHoras.Add(new RegistroHorasEntity(
            Empleado, new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null, null, null,
            proyecto.Id, "Banco Nacional", "Core Bancario",
            "Remoto", "Consultor", "Descripción original", "Bogota"));
        await context.SaveChangesAsync(CancellationToken.None);

        return context;
    }
}
