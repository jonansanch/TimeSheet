using ApexCharts;
using KPG.Timesheet.WebUI;
using KPG.Timesheet.WebUI.Infrastructure.Repositories;
using KPG.Timesheet.WebUI.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl no está configurada.");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

builder.Services.AddMudServices();
builder.Services.AddApexCharts();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IRegistroHorasRepository, RegistroHorasRepository>();
builder.Services.AddScoped<IParametroSistemaRepository, ParametroSistemaRepository>();
builder.Services.AddScoped<ISolicitudExcepcionRepository, SolicitudExcepcionRepository>();
builder.Services.AddScoped<ISolicitudExcepcionAdminRepository, SolicitudExcepcionAdminRepository>();
builder.Services.AddScoped<IUserAdminRepository, UserAdminRepository>();
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IModalidadRepository, ModalidadRepository>();
builder.Services.AddScoped<ILugarTrabajoRepository, LugarTrabajoRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IReportesRepository, ReportesRepository>();
builder.Services.AddScoped<INotificacionesRepository, NotificacionesRepository>();
builder.Services.AddSingleton<AuthStateService>();
builder.Services.AddScoped<KpgAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<KpgAuthStateProvider>());
builder.Services.AddScoped<CurrentUserService>();

await builder.Build().RunAsync();
