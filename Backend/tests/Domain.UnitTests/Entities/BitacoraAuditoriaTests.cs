using FluentAssertions;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using Xunit;

namespace KPG.Timesheet.Domain.UnitTests.Entities;

public class BitacoraAuditoriaTests
{
    [Fact]
    public void Crear_ConDatosCompletos_AsignaPropiedadesCorrectamente()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var metadata = new { Email = "test@kpg.com" };

        var entrada = BitacoraAuditoria.Crear(
            TipoEventoBitacora.LoginExitoso,
            "user-id", "user@kpg.com",
            "AspNetUsers", "user-id",
            metadata, timestamp);

        entrada.TipoEvento.Should().Be(TipoEventoBitacora.LoginExitoso);
        entrada.ActorId.Should().Be("user-id");
        entrada.ActorEmail.Should().Be("user@kpg.com");
        entrada.EntidadAfectada.Should().Be("AspNetUsers");
        entrada.EntidadId.Should().Be("user-id");
        entrada.Timestamp.Should().Be(timestamp);
        entrada.MetadataJson.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Crear_SinMetadata_MetadataJsonEsNull()
    {
        var entrada = BitacoraAuditoria.Crear(
            TipoEventoBitacora.BajaUsuario,
            "admin-id", null,
            "AspNetUsers", "target-id",
            null, DateTimeOffset.UtcNow);

        entrada.MetadataJson.Should().BeNull();
    }

    [Fact]
    public void TipoEventoBitacora_TodasLasConstantesNoVacias()
    {
        var constantes = new[]
        {
            TipoEventoBitacora.LoginExitoso,
            TipoEventoBitacora.AltaUsuario,
            TipoEventoBitacora.BajaUsuario,
            TipoEventoBitacora.ReactivacionUsuario,
            TipoEventoBitacora.EliminacionUsuario,
            TipoEventoBitacora.CambioRol,
            TipoEventoBitacora.AprobacionExcepcion,
            TipoEventoBitacora.RechazoExcepcion,
            TipoEventoBitacora.ModificacionDescripcion,
            TipoEventoBitacora.EliminacionRegistro,
            TipoEventoBitacora.NotificacionEnviada
        };

        constantes.Should().AllSatisfy(c => c.Should().NotBeNullOrWhiteSpace());
    }

    [Fact]
    public void TipoEventoBitacora_TodasLasConstantesUnicas()
    {
        var constantes = new[]
        {
            TipoEventoBitacora.LoginExitoso,
            TipoEventoBitacora.AltaUsuario,
            TipoEventoBitacora.BajaUsuario,
            TipoEventoBitacora.ReactivacionUsuario,
            TipoEventoBitacora.EliminacionUsuario,
            TipoEventoBitacora.CambioRol,
            TipoEventoBitacora.AprobacionExcepcion,
            TipoEventoBitacora.RechazoExcepcion,
            TipoEventoBitacora.ModificacionDescripcion,
            TipoEventoBitacora.EliminacionRegistro,
            TipoEventoBitacora.NotificacionEnviada
        };

        constantes.Should().OnlyHaveUniqueItems();
    }
}
