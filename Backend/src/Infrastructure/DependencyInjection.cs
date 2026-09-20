using System.Text;
using Dapper;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Models;
using KPG.Timesheet.Infrastructure.Bitacora;
using KPG.Timesheet.Infrastructure.Dashboard;
using KPG.Timesheet.Infrastructure.Data;
using KPG.Timesheet.Infrastructure.Data.Interceptors;
using KPG.Timesheet.Infrastructure.Notificaciones;
using KPG.Timesheet.Infrastructure.Reportes;
using KPG.Timesheet.Infrastructure.Email;
using KPG.Timesheet.Infrastructure.Identity;
using KPG.Timesheet.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
        SqlMapper.AddTypeHandler(new NullableDateTimeOffsetTypeHandler());

        var raw = builder.Configuration.GetConnectionString("DefaultConnection");
        Guard.Against.Null(raw, message: "Connection string 'DefaultConnection' not found.");

        // Construir la connection string y aplicar excepciones seguras en desarrollo
        var sqlBuilder = new SqlConnectionStringBuilder(raw);
        if (builder.Environment.IsDevelopment())
        {
            // Permitir confiar en el certificado del servidor solo en desarrollo
            sqlBuilder.TrustServerCertificate = true;
            // Mantener cifrado activo
            sqlBuilder.Encrypt = true;
        }

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, RegistroHorasImmutabilityInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            // Azure SQL Serverless se auto-pausa: el primer acceso tras la pausa
            // tarda en reanudar, hay que reintentar en vez de fallar.
            options.UseSqlServer(sqlBuilder.ConnectionString, sql =>
                sql.EnableRetryOnFailure(
                    maxRetryCount: 6,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null));
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<System.Data.IDbConnection>(_ =>
            new Microsoft.Data.SqlClient.SqlConnection(sqlBuilder.ConnectionString));

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT configuration section 'Jwt' not found.");
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
            });

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IClock, SystemClock>();
        builder.Services.AddTransient<IIdentityService, IdentityService>();
        builder.Services.AddTransient<IJwtTokenService, JwtTokenService>();

        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
        builder.Services.AddTransient<IEmailService, SmtpEmailService>();
        builder.Services.AddScoped<IBitacoraService, BitacoraService>();

        builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
        builder.Services.AddScoped<IBitacoraQueryRepository, BitacoraQueryRepository>();
        builder.Services.AddScoped<INotificacionesRepository, NotificacionesRepository>();
        builder.Services.AddScoped<IReportesRepository, ReportesRepository>();
        builder.Services.AddScoped<ICadenaAprobacionService, KPG.Timesheet.Infrastructure.Organizacion.CadenaAprobacionService>();
        builder.Services.AddScoped<IAprobacionesRepository, KPG.Timesheet.Infrastructure.Organizacion.AprobacionesRepository>();
        builder.Services.AddScoped<INotificadorAprobacion, KPG.Timesheet.Infrastructure.Notificaciones.NotificadorAprobacion>();
        builder.Services.AddScoped<ITimesheetImportParser, TimesheetImportParser>();

        // Interpretacion de voz por IA. La key llega por variable de entorno; sin ella el
        // servicio queda "no disponible" y el frontend usa su parser de reglas.
        builder.Services.Configure<KPG.Timesheet.Infrastructure.Voz.AnthropicSettings>(
            builder.Configuration.GetSection("Anthropic"));
        builder.Services.AddScoped<IInterpreteVoz, KPG.Timesheet.Infrastructure.Voz.ClaudeInterpreteVoz>();

        // Registrar handlers de MediatR que viven en Infrastructure (export handlers: Excel/PDF)
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(SmtpEmailService).Assembly));
    }
}
