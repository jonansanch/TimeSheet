using System.Reflection;
using KPG.Timesheet.Application.Common.Behaviours;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Services;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Services.AddScoped<IParametrosSistemaService, ParametrosSistemaService>();
        builder.Services.AddScoped<IVentanaRetroactividadService, VentanaRetroactividadService>();
        builder.Services.AddScoped<IRestriccionDiaService, RestriccionDiaService>();
        builder.Services.AddScoped<IValidadorDescripcion, ValidadorDescripcionService>();

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });
    }
}
