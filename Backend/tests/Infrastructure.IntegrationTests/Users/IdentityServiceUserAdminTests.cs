using Dapper;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Infrastructure.Data;
using KPG.Timesheet.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Users;

public class IdentityServiceUserAdminTests
{
    [Fact]
    public async Task CreateUserAsync_WithRole_CreatesActiveUserAndAssignsRole()
    {
        var services = CreateServices();
        var identity = services.GetRequiredService<IdentityService>();

        var (result, user) = await identity.CreateUserAsync("nuevo@kpg.com", "Empleado1234!", Roles.Empleado);

        result.Succeeded.Should().BeTrue();
        user.Should().NotBeNull();
        user!.IsActive.Should().BeTrue();
        user.Role.Should().Be(Roles.Empleado);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WhenUserInactive_ReturnsNull()
    {
        var services = CreateServices();
        var identity = services.GetRequiredService<IdentityService>();
        var (result, user) = await identity.CreateUserAsync("inactivo@kpg.com", "Empleado1234!", Roles.Empleado);
        result.Succeeded.Should().BeTrue();

        await identity.DeactivateUserAsync(user!.Id, "admin-id");

        var credentials = await identity.ValidateCredentialsAsync("inactivo@kpg.com", "Empleado1234!");
        credentials.Should().BeNull();
    }

    [Fact]
    public async Task ActivateUserAsync_WhenUserInactive_AllowsCredentialsAgain()
    {
        var services = CreateServices();
        var identity = services.GetRequiredService<IdentityService>();
        var (result, user) = await identity.CreateUserAsync("reactivar@kpg.com", "Empleado1234!", Roles.Empleado);
        result.Succeeded.Should().BeTrue();

        await identity.DeactivateUserAsync(user!.Id, "admin-id");
        await identity.ActivateUserAsync(user.Id);

        var credentials = await identity.ValidateCredentialsAsync("reactivar@kpg.com", "Empleado1234!");
        credentials.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUsersAsync_DoesNotExposeSensitiveFieldsAndPaginates()
    {
        var services = CreateServices();
        var identity = services.GetRequiredService<IdentityService>();
        await identity.CreateUserAsync("b@kpg.com", "Empleado1234!", Roles.Empleado);
        await identity.CreateUserAsync("a@kpg.com", "Empleado1234!", Roles.Admin);

        var page = await identity.GetUsersAsync(1, 1, "email", false);

        page.TotalCount.Should().Be(2);
        page.Items.Should().HaveCount(1);
        page.Items[0].Email.Should().Be("a@kpg.com");
    }

    [Fact]
    public async Task ChangeUserRoleAsync_WhenValid_ReplacesPreviousRole()
    {
        var services = CreateServices();
        var identity = services.GetRequiredService<IdentityService>();
        var (createResult, user) = await identity.CreateUserAsync("rol@kpg.com", "Empleado1234!", Roles.Empleado);
        createResult.Succeeded.Should().BeTrue();

        var (result, updatedUser) = await identity.ChangeUserRoleAsync(user!.Id, Roles.Supervisor);

        result.Succeeded.Should().BeTrue();
        updatedUser.Should().NotBeNull();
        updatedUser!.Role.Should().Be(Roles.Supervisor);
        var credentials = await identity.ValidateCredentialsAsync("rol@kpg.com", "Empleado1234!");
        credentials!.Roles.Should().BeEquivalentTo([Roles.Supervisor]);
    }

    [Fact]
    public async Task ChangeUserRoleAsync_WhenRoleInvalid_DoesNotMutateCurrentRole()
    {
        var services = CreateServices();
        var identity = services.GetRequiredService<IdentityService>();
        var (createResult, user) = await identity.CreateUserAsync("rol-invalido@kpg.com", "Empleado1234!", Roles.Empleado);
        createResult.Succeeded.Should().BeTrue();

        var (result, updatedUser) = await identity.ChangeUserRoleAsync(user!.Id, "Root");

        result.Succeeded.Should().BeFalse();
        updatedUser.Should().BeNull();
        var credentials = await identity.ValidateCredentialsAsync("rol-invalido@kpg.com", "Empleado1234!");
        credentials!.Roles.Should().BeEquivalentTo([Roles.Empleado]);
    }

    [Fact]
    public async Task ChangeUserRoleAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        var services = CreateServices();
        var identity = services.GetRequiredService<IdentityService>();

        var (result, updatedUser) = await identity.ChangeUserRoleAsync("missing-user", Roles.Supervisor);

        result.Succeeded.Should().BeFalse();
        updatedUser.Should().BeNull();
    }

    [Fact]
    public async Task ChangeUserRoleAsync_WhenUserInactive_UpdatesRoleButKeepsInactive()
    {
        var services = CreateServices();
        var identity = services.GetRequiredService<IdentityService>();
        var (createResult, user) = await identity.CreateUserAsync("inactivo-rol@kpg.com", "Empleado1234!", Roles.Empleado);
        createResult.Succeeded.Should().BeTrue();
        await identity.DeactivateUserAsync(user!.Id, "admin-id");

        var (result, updatedUser) = await identity.ChangeUserRoleAsync(user.Id, Roles.Gerente);

        result.Succeeded.Should().BeTrue();
        updatedUser!.Role.Should().Be(Roles.Gerente);
        updatedUser.IsActive.Should().BeFalse();
        var credentials = await identity.ValidateCredentialsAsync("inactivo-rol@kpg.com", "Empleado1234!");
        credentials.Should().BeNull();
    }

    [Fact]
    public async Task ChangeUserRoleAsync_WhenLastActiveAdminWouldBeRemoved_ReturnsFailure()
    {
        var services = CreateServices();
        var identity = services.GetRequiredService<IdentityService>();
        var (createResult, admin) = await identity.CreateUserAsync("admin-unico@kpg.com", "Empleado1234!", Roles.Admin);
        createResult.Succeeded.Should().BeTrue();

        var (result, updatedUser) = await identity.ChangeUserRoleAsync(admin!.Id, Roles.Empleado);

        result.Succeeded.Should().BeFalse();
        updatedUser.Should().BeNull();
        var credentials = await identity.ValidateCredentialsAsync("admin-unico@kpg.com", "Empleado1234!");
        credentials!.Roles.Should().BeEquivalentTo([Roles.Admin]);
    }

    [Fact]
    public async Task ChangeUserRoleAsync_WhenAnotherActiveAdminExists_AllowsAdminRoleChange()
    {
        var services = CreateServices();
        var identity = services.GetRequiredService<IdentityService>();
        var (firstResult, admin) = await identity.CreateUserAsync("admin-cambio@kpg.com", "Empleado1234!", Roles.Admin);
        var (secondResult, _) = await identity.CreateUserAsync("admin-respaldo@kpg.com", "Empleado1234!", Roles.Admin);
        firstResult.Succeeded.Should().BeTrue();
        secondResult.Succeeded.Should().BeTrue();

        var (result, updatedUser) = await identity.ChangeUserRoleAsync(admin!.Id, Roles.Empleado);

        result.Succeeded.Should().BeTrue();
        updatedUser!.Role.Should().Be(Roles.Empleado);
    }

    private static ServiceProvider CreateServices()
    {
        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
        SqlMapper.AddTypeHandler(new NullableDateTimeOffsetTypeHandler());

        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>>();
        services.AddTransient<IdentityService>();
        services.AddSingleton<System.Data.IDbConnection>(connection);

        var provider = services.BuildServiceProvider();
        var context = provider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        foreach (var role in new[] { Roles.Admin, Roles.Gerente, Roles.Supervisor, Roles.Empleado })
            context.Roles.Add(new IdentityRole(role) { NormalizedName = role.ToUpperInvariant() });
        context.SaveChanges();

        return provider;
    }
}
