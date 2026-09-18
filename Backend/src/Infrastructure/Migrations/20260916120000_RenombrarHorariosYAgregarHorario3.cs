using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPG.Timesheet.Infrastructure.Migrations;

/// <summary>
/// Renombra los bloques AM/PM a horarios numerados y agrega un tercer bloque.
/// El rename preserva los datos existentes: no hay copia ni borrado de filas.
/// </summary>
public partial class RenombrarHorariosYAgregarHorario3 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn("HoraEntradaAM", "RegistrosHoras", "HoraEntrada1");
        migrationBuilder.RenameColumn("HoraSalidaAM",  "RegistrosHoras", "HoraSalida1");
        migrationBuilder.RenameColumn("HoraEntradaPM", "RegistrosHoras", "HoraEntrada2");
        migrationBuilder.RenameColumn("HoraSalidaPM",  "RegistrosHoras", "HoraSalida2");

        migrationBuilder.AddColumn<TimeOnly>("HoraEntrada3", "RegistrosHoras", nullable: true);
        migrationBuilder.AddColumn<TimeOnly>("HoraSalida3",  "RegistrosHoras", nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Los registros que solo tengan horario 3 pierden sus horas al revertir:
        // el esquema anterior no tiene donde alojarlas.
        migrationBuilder.DropColumn("HoraEntrada3", "RegistrosHoras");
        migrationBuilder.DropColumn("HoraSalida3",  "RegistrosHoras");

        migrationBuilder.RenameColumn("HoraEntrada1", "RegistrosHoras", "HoraEntradaAM");
        migrationBuilder.RenameColumn("HoraSalida1",  "RegistrosHoras", "HoraSalidaAM");
        migrationBuilder.RenameColumn("HoraEntrada2", "RegistrosHoras", "HoraEntradaPM");
        migrationBuilder.RenameColumn("HoraSalida2",  "RegistrosHoras", "HoraSalidaPM");
    }
}
