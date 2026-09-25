using KPG.Timesheet.Application.Common.Models;
using KPG.Timesheet.Infrastructure.Data;
using KPG.Timesheet.Infrastructure.Identity;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Data;

public class NacionalidadDemoTests
{
    [Fact]
    public void SeleccionarCodigoPaisDemo_IsStableAndReturnsValidIsoCode()
    {
        var primero = ApplicationDbContextInitialiser.SeleccionarCodigoPaisDemo("persona@kpg.com");
        var segundo = ApplicationDbContextInitialiser.SeleccionarCodigoPaisDemo(" PERSONA@KPG.COM ");

        segundo.Should().Be(primero);
        CodigoPaisIso.EsValido(primero).Should().BeTrue();
    }

    [Fact]
    public void SeleccionarCodigoPaisDemo_ProducesVariedCountries()
    {
        var codigos = Enumerable.Range(1, 40)
            .Select(numero => ApplicationDbContextInitialiser.SeleccionarCodigoPaisDemo($"persona{numero}@kpg.com"))
            .Distinct()
            .ToList();

        codigos.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void AsignarNacionalidadesDemo_PreservesExistingCountryAndIsIdempotent()
    {
        var existente = new ApplicationUser { Id = "1", Email = "existente@kpg.com", CodigoPais = "CO" };
        var pendiente = new ApplicationUser { Id = "2", Email = "pendiente@kpg.com" };
        var usuarios = new[] { existente, pendiente };

        ApplicationDbContextInitialiser.AsignarNacionalidadesDemo(usuarios).Should().Be(1);
        var asignado = pendiente.CodigoPais;
        ApplicationDbContextInitialiser.AsignarNacionalidadesDemo(usuarios).Should().Be(0);

        existente.CodigoPais.Should().Be("CO");
        pendiente.CodigoPais.Should().Be(asignado);
        CodigoPaisIso.EsValido(asignado).Should().BeTrue();
    }

    [Fact]
    public void AsignarNacionalidadesDemo_AssignsNullEmptyAndWhitespaceValues()
    {
        var usuarios = new[]
        {
            new ApplicationUser { Id = "1", Email = "null@kpg.com", CodigoPais = null },
            new ApplicationUser { Id = "2", Email = "empty@kpg.com", CodigoPais = "" },
            new ApplicationUser { Id = "3", Email = "space@kpg.com", CodigoPais = "   " }
        };

        ApplicationDbContextInitialiser.AsignarNacionalidadesDemo(usuarios).Should().Be(3);
        usuarios.Should().OnlyContain(user => CodigoPaisIso.EsValido(user.CodigoPais));
        usuarios.Should().OnlyContain(user => !string.IsNullOrWhiteSpace(user.CodigoPais));
    }
}
