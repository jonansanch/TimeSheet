using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPG.Timesheet.Infrastructure.Migrations;

/// <inheritdoc />
public partial class MergeRegistroHorasDiario : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Agregar nuevas columnas nullable
        migrationBuilder.AddColumn<TimeOnly>("HoraEntradaAM", "RegistrosHoras", nullable: true);
        migrationBuilder.AddColumn<TimeOnly>("HoraSalidaAM",  "RegistrosHoras", nullable: true);
        migrationBuilder.AddColumn<TimeOnly>("HoraEntradaPM", "RegistrosHoras", nullable: true);
        migrationBuilder.AddColumn<TimeOnly>("HoraSalidaPM",  "RegistrosHoras", nullable: true);

        // 2. Migrar datos: copiar HoraEntrada/HoraSalida según Turno
        migrationBuilder.Sql("""
            UPDATE RegistrosHoras
            SET HoraEntradaAM = HoraEntrada, HoraSalidaAM = HoraSalida
            WHERE Turno = 'AM';
            """);

        migrationBuilder.Sql("""
            UPDATE RegistrosHoras
            SET HoraEntradaPM = HoraEntrada, HoraSalidaPM = HoraSalida
            WHERE Turno = 'PM';
            """);

        // 3. Fusionar registros PM en el registro AM del mismo día/usuario
        migrationBuilder.Sql("""
            UPDATE am_rec
            SET am_rec.HoraEntradaPM = pm_rec.HoraEntradaPM,
                am_rec.HoraSalidaPM  = pm_rec.HoraSalidaPM
            FROM RegistrosHoras am_rec
            INNER JOIN RegistrosHoras pm_rec
                ON am_rec.UserId = pm_rec.UserId
               AND am_rec.FechaRegistro = pm_rec.FechaRegistro
               AND am_rec.Turno = 'AM'
               AND pm_rec.Turno = 'PM';
            """);

        // 4. Eliminar los registros PM que ya fueron fusionados
        migrationBuilder.Sql("""
            DELETE FROM RegistrosHoras
            WHERE Turno = 'PM'
              AND EXISTS (
                  SELECT 1 FROM RegistrosHoras am
                  WHERE am.UserId = RegistrosHoras.UserId
                    AND am.FechaRegistro = RegistrosHoras.FechaRegistro
                    AND am.Turno = 'AM'
              );
            """);

        // 5. Para registros PM sin contraparte AM: ya tienen HoraEntradaPM/HoraSalidaPM asignados en paso 2.
        //    No requieren acción adicional.

        // 6. Eliminar índice existente y columnas antiguas
        migrationBuilder.DropIndex("IX_RegistrosHoras_FechaRegistro", "RegistrosHoras");
        migrationBuilder.DropColumn("Turno",       "RegistrosHoras");
        migrationBuilder.DropColumn("HoraEntrada", "RegistrosHoras");
        migrationBuilder.DropColumn("HoraSalida",  "RegistrosHoras");

        // 7. Índice único por usuario/día
        migrationBuilder.CreateIndex(
            "IX_RegistrosHoras_UserId_FechaRegistro",
            "RegistrosHoras",
            ["UserId", "FechaRegistro"],
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_RegistrosHoras_UserId_FechaRegistro", "RegistrosHoras");

        migrationBuilder.AddColumn<string>("Turno",       "RegistrosHoras", maxLength: 2, nullable: false, defaultValue: "AM");
        migrationBuilder.AddColumn<TimeOnly>("HoraEntrada", "RegistrosHoras", nullable: false, defaultValue: new TimeOnly(8, 0));
        migrationBuilder.AddColumn<TimeOnly>("HoraSalida",  "RegistrosHoras", nullable: false, defaultValue: new TimeOnly(13, 0));

        // Restaurar datos AM
        migrationBuilder.Sql("""
            UPDATE RegistrosHoras
            SET Turno = 'AM', HoraEntrada = HoraEntradaAM, HoraSalida = HoraSalidaAM
            WHERE HoraEntradaAM IS NOT NULL;
            """);

        // Insertar registros PM separados para los que tenían PM
        migrationBuilder.Sql("""
            INSERT INTO RegistrosHoras
                (UserId, FechaRegistro, Turno, HoraEntrada, HoraSalida,
                 Cliente, Proyecto, Modalidad, Recurso, Descripcion, Lugar, EsRetroactivo,
                 Created, CreatedBy, LastModified, LastModifiedBy)
            SELECT UserId, FechaRegistro, 'PM', HoraEntradaPM, HoraSalidaPM,
                   Cliente, Proyecto, Modalidad, Recurso, Descripcion, Lugar, EsRetroactivo,
                   Created, CreatedBy, LastModified, LastModifiedBy
            FROM RegistrosHoras
            WHERE HoraEntradaPM IS NOT NULL;
            """);

        migrationBuilder.DropColumn("HoraEntradaAM", "RegistrosHoras");
        migrationBuilder.DropColumn("HoraSalidaAM",  "RegistrosHoras");
        migrationBuilder.DropColumn("HoraEntradaPM", "RegistrosHoras");
        migrationBuilder.DropColumn("HoraSalidaPM",  "RegistrosHoras");

        migrationBuilder.CreateIndex("IX_RegistrosHoras_FechaRegistro", "RegistrosHoras", "FechaRegistro");
    }
}
