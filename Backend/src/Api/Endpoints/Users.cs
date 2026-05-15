using KPG.Timesheet.Application.Features.Users.Commands.ActivateUser;
using KPG.Timesheet.Application.Features.Users.Commands.ChangeUserRole;
using KPG.Timesheet.Application.Features.Users.Commands.CreateUser;
using KPG.Timesheet.Application.Features.Users.Commands.DeactivateUser;
using KPG.Timesheet.Application.Features.Users.Commands.DeleteUser;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class Users : IEndpointGroup
{
    public static string RoutePrefix => "/api/users";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var adminOnly = new AuthorizeAttribute { Roles = Roles.Admin };

        groupBuilder.MapGet(GetUsers).RequireAuthorization(adminOnly);
        groupBuilder.MapPost(CreateUser).RequireAuthorization(adminOnly);
        groupBuilder.MapPost(ActivateUser, "{id}/activate").RequireAuthorization(adminOnly);
        groupBuilder.MapPost(DeactivateUser, "{id}/deactivate").RequireAuthorization(adminOnly);
        groupBuilder.MapPut(ChangeUserRole, "{id}/role").RequireAuthorization(adminOnly);
        groupBuilder.MapDelete(DeleteUser, "{id}").RequireAuthorization(adminOnly);
    }

    [EndpointSummary("Listar usuarios")]
    [EndpointDescription("Retorna usuarios de Identity para administracion, paginados y sin datos sensibles.")]
    [ProducesResponseType<UsersPageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetUsers(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? sortBy = "email",
        [FromQuery] bool sortDescending = false)
    {
        var result = await sender.Send(new GetUsersQuery(pageNumber, pageSize, sortBy, sortDescending), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Crear usuario")]
    [EndpointDescription("Crea una cuenta activa con email, password y rol inicial.")]
    [ProducesResponseType<UserAdminDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> CreateUser(
        [FromBody] CreateUserCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/users/{result.Id}", result);
    }

    [EndpointSummary("Activar usuario")]
    [ProducesResponseType<UserAdminDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> ActivateUser(
        string id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ActivateUserCommand(id), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Desactivar usuario")]
    [ProducesResponseType<UserAdminDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> DeactivateUser(
        string id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeactivateUserCommand(id), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Cambiar rol de usuario")]
    [EndpointDescription("Reemplaza el rol principal de un usuario existente por un rol permitido.")]
    [ProducesResponseType<UserAdminDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> ChangeUserRole(
        string id,
        [FromBody] ChangeUserRoleRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangeUserRoleCommand(id, request.Role), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Eliminar usuario")]
    [EndpointDescription("Elimina fisicamente cuentas sin historia; si hay historia, conserva la cuenta inactiva.")]
    [ProducesResponseType<DeleteUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> DeleteUser(
        string id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteUserCommand(id), cancellationToken);
        return Results.Ok(result);
    }
}

public record ChangeUserRoleRequest(string Role);
