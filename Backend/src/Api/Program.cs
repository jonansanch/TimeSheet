using KPG.Timesheet.Infrastructure.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors(static builder =>
    builder.AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin());

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePages(async ctx =>
{
    var response = ctx.HttpContext.Response;
    if (response.StatusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden
        && !response.HasStarted)
    {
        response.ContentType = "application/problem+json";
        await response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc9457",
            title = response.StatusCode == StatusCodes.Status401Unauthorized
                ? "No autenticado."
                : "Acceso denegado.",
            status = response.StatusCode
        });
    }
});

app.MapOpenApi();
app.MapScalarApiReference();

app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/scalar"));

app.MapEndpoints(typeof(Program).Assembly);

await app.RunAsync();
