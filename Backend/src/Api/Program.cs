using KPG.Timesheet.Infrastructure.Data;
using KPG.Timesheet.Infrastructure.Jobs;
using QuestPDF.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration));

builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();
builder.Services.AddHostedService<NotificacionesPendientesJob>();

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
app.UseSerilogRequestLogging(opts =>
    opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} → {StatusCode} en {Elapsed:0.0000} ms");
// Origenes permitidos via configuracion (Cors:AllowedOrigins).
// Sin valores configurados se abre a cualquier origen, para no romper desarrollo local.
var allowedOrigins = app.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
app.UseCors(policy =>
{
    policy.AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders("Content-Disposition");

    if (allowedOrigins.Length > 0)
    {
        policy.WithOrigins(allowedOrigins);
    }
    else
    {
        policy.AllowAnyOrigin();
    }
});

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

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

public partial class Program { }
