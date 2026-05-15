using KPG.Timesheet.Application.Features.Catalogos.Modalidades.Commands.CreateModalidad;
using KPG.Timesheet.Application.Features.Catalogos.Modalidades.Commands.ToggleModalidadActiva;
using KPG.Timesheet.Application.Features.Catalogos.Modalidades.Commands.UpdateModalidad;
using KPG.Timesheet.Application.Features.Catalogos.Modalidades.Queries.GetModalidades;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class Modalidades : IEndpointGroup
{
    public static string RoutePrefix => "/api/modalidades";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var adminOnly = new AuthorizeAttribute { Roles = Roles.Admin };
        var anyAuth = new AuthorizeAttribute
        {
            Roles = $"{Roles.Empleado},{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}"
        };

        groupBuilder.MapGet("", GetAll).RequireAuthorization(adminOnly);
        groupBuilder.MapGet("activas", GetActivas).RequireAuthorization(anyAuth);
        groupBuilder.MapPost("", Create).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("{id:int}", Update).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("{id:int}/toggle", Toggle).RequireAuthorization(adminOnly);
    }

    [EndpointSummary("Listar modalidades")]
    [ProducesResponseType<List<ModalidadDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetAll(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetModalidadesQuery(SoloActivas: false), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Listar modalidades activas")]
    [ProducesResponseType<List<ModalidadDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> GetActivas(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetModalidadesQuery(SoloActivas: true), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Crear modalidad")]
    [ProducesResponseType<ModalidadDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> Create(
        [FromBody] CreateModalidadCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/modalidades/{result.Id}", result);
    }

    [EndpointSummary("Actualizar nombre de modalidad")]
    [ProducesResponseType<ModalidadDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Update(
        int id,
        [FromBody] UpdateModalidadRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateModalidadCommand(id, request.Nombre), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Activar o desactivar modalidad")]
    [ProducesResponseType<ModalidadDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Toggle(
        int id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ToggleModalidadActivaCommand(id), cancellationToken);
        return Results.Ok(result);
    }
}

public record UpdateModalidadRequest(string Nombre);
