using KPG.Timesheet.Api.Endpoints;
using KPG.Timesheet.Application.Features.Users.Commands.AsignarEstructura;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;
using MediatR;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Users;

public class UsersEndpointMappingTests
{
    [Fact]
    public void AsignarEstructuraRequest_WhenLegacyJsonOmitsCountry_PreservesUpdateFlagAsFalse()
    {
        var request = JsonSerializer.Deserialize<KPG.Timesheet.Api.Endpoints.AsignarEstructuraRequest>(
            """{"supervisorUserId":null,"puestoId":null}""",
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        request.Should().NotBeNull();
        request!.ActualizarCodigoPais.Should().BeFalse();
        request.CodigoPais.Should().BeNull();
    }

    [Fact]
    public async Task AsignarEstructura_MapsCountryAndExplicitUpdateFlag()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<AsignarEstructuraUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(new UserAdminDto(
                "user-id", "user@kpg.com", "User", true, "Empleado",
                DateTimeOffset.UtcNow, null, null, null, null, null, "AQ"));
        var request = new KPG.Timesheet.Api.Endpoints.AsignarEstructuraRequest(
            null, null, "AQ", ActualizarCodigoPais: true);

        await KPG.Timesheet.Api.Endpoints.Users.AsignarEstructura(
            "user-id", request, sender, CancellationToken.None);

        await sender.Received(1).Send(
            Arg.Is<AsignarEstructuraUsuarioCommand>(c =>
                c.UserId == "user-id" && c.CodigoPais == "AQ" && c.ActualizarCodigoPais),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AsignarEstructura_WhenCountryIsOmitted_MapsPreserveSemantics()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<AsignarEstructuraUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(new UserAdminDto(
                "user-id", "user@kpg.com", "User", true, "Empleado",
                DateTimeOffset.UtcNow, null, null, null, null, null, "CO"));
        var request = new KPG.Timesheet.Api.Endpoints.AsignarEstructuraRequest(null, null);

        await KPG.Timesheet.Api.Endpoints.Users.AsignarEstructura(
            "user-id", request, sender, CancellationToken.None);

        await sender.Received(1).Send(
            Arg.Is<AsignarEstructuraUsuarioCommand>(c => !c.ActualizarCodigoPais),
            Arg.Any<CancellationToken>());
    }
}
