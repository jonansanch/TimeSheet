using KPG.Timesheet.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace KPG.Timesheet.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly string _frontendBaseUrl;

    public ForgotPasswordCommandHandler(
        IIdentityService identityService,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _identityService = identityService;
        _emailService = emailService;
        _frontendBaseUrl = configuration["FrontendBaseUrl"] ?? "http://localhost:5200";
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var (found, token, email) = await _identityService.GeneratePasswordResetTokenAsync(request.Email);

        // Siempre retorna sin error para no revelar si el email existe
        if (!found || token is null || email is null)
            return;

        var encodedToken = Uri.EscapeDataString(token);
        var resetUrl = $"{_frontendBaseUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={encodedToken}";

        var body = $"""
            Hola,

            Recibimos una solicitud para restablecer la contraseña de tu cuenta en KPG Timesheet.

            Haz clic en el siguiente enlace para establecer una nueva contraseña:
            {resetUrl}

            Este enlace es válido por 24 horas.

            Si no solicitaste este cambio, puedes ignorar este correo.

            — KPG Timesheet
            """;

        await _emailService.SendAsync(email, "Restablecer contraseña — KPG Timesheet", body, cancellationToken);
    }
}
