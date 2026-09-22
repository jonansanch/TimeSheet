using KPG.Timesheet.Application.Common.Services;
using KPG.Timesheet.Application.Features.Sistema.Commands.UpdateLogoReportes;
using KPG.Timesheet.Application.Features.Sistema.Queries.GetLogoReportes;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Sistema;

public class LogoReportesTests
{
    // 1x1 PNG transparente valido, en base64.
    private const string PngValidoBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";

    [Fact]
    public async Task GetLogoReportes_SinConfigurar_ReturnsEmpty()
    {
        await using var context = CreateContext();
        var handler = new GetLogoReportesQueryHandler(new ParametrosSistemaService(context));

        var result = await handler.Handle(new GetLogoReportesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateLogoReportes_ConPngValido_Persiste()
    {
        await using var context = CreateContext();
        var dataUri = $"data:image/png;base64,{PngValidoBase64}";

        new UpdateLogoReportesCommandValidator()
            .Validate(new UpdateLogoReportesCommand(dataUri)).IsValid.Should().BeTrue();

        var handler = new UpdateLogoReportesCommandHandler(context);
        await handler.Handle(new UpdateLogoReportesCommand(dataUri), CancellationToken.None);

        var getHandler = new GetLogoReportesQueryHandler(new ParametrosSistemaService(context));
        var result = await getHandler.Handle(new GetLogoReportesQuery(), CancellationToken.None);

        result.Should().Be(dataUri);
    }

    [Fact]
    public async Task UpdateLogoReportes_ConNulo_LoQuita()
    {
        await using var context = CreateContext();
        var handler = new UpdateLogoReportesCommandHandler(context);
        await handler.Handle(
            new UpdateLogoReportesCommand($"data:image/png;base64,{PngValidoBase64}"), CancellationToken.None);

        await handler.Handle(new UpdateLogoReportesCommand(null), CancellationToken.None);

        var getHandler = new GetLogoReportesQueryHandler(new ParametrosSistemaService(context));
        var result = await getHandler.Handle(new GetLogoReportesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("data:application/pdf;base64,JVBERi0xLjQK")]
    [InlineData("no-es-un-data-uri")]
    [InlineData("data:image/gif;base64,R0lGODlh")]
    public void Validator_ConDataUriInvalido_Falla(string dataUri)
    {
        var result = new UpdateLogoReportesCommandValidator()
            .Validate(new UpdateLogoReportesCommand(dataUri));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_ConImagenDemasiadoGrande_Falla()
    {
        var enorme = "data:image/png;base64," + new string('A', UpdateLogoReportesCommandValidator.MaxLargoDataUri);

        var result = new UpdateLogoReportesCommandValidator()
            .Validate(new UpdateLogoReportesCommand(enorme));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void LogoDataUri_DecodeBytes_DecodificaCorrectamente()
    {
        var dataUri = $"data:image/png;base64,{PngValidoBase64}";

        var bytes = KPG.Timesheet.Infrastructure.Reportes.LogoDataUri.DecodeBytes(dataUri);

        bytes.Should().NotBeNull();
        bytes!.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void LogoDataUri_DecodeFormat_DetectaPngYJpeg()
    {
        KPG.Timesheet.Infrastructure.Reportes.LogoDataUri.DecodeFormat("data:image/png;base64,abc")
            .Should().Be(ClosedXML.Excel.Drawings.XLPictureFormat.Png);
        KPG.Timesheet.Infrastructure.Reportes.LogoDataUri.DecodeFormat("data:image/jpeg;base64,abc")
            .Should().Be(ClosedXML.Excel.Drawings.XLPictureFormat.Jpeg);
        KPG.Timesheet.Infrastructure.Reportes.LogoDataUri.DecodeFormat(null).Should().BeNull();
        KPG.Timesheet.Infrastructure.Reportes.LogoDataUri.DecodeFormat("no-es-data-uri").Should().BeNull();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
