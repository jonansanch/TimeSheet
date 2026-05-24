using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPG.Timesheet.Infrastructure.Migrations;

/// <inheritdoc />
[DbContext(typeof(ApplicationDbContext))]
[Migration("20260523090000_AddPerformanceIndexes")]
public partial class AddPerformanceIndexes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_RegistrosHoras_UserId_FechaRegistro'
                  AND object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
            )
            BEGIN
                DROP INDEX [IX_RegistrosHoras_UserId_FechaRegistro] ON [dbo].[RegistrosHoras];
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_RegistrosHoras_UserId_FechaRegistro_Cliente_Proyecto'
                  AND object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
            )
            BEGIN
                CREATE UNIQUE INDEX [IX_RegistrosHoras_UserId_FechaRegistro_Cliente_Proyecto]
                    ON [dbo].[RegistrosHoras] ([UserId], [FechaRegistro], [Cliente], [Proyecto]);
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_RegistrosHoras_FechaRegistro'
                  AND object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
            )
            BEGIN
                CREATE INDEX [IX_RegistrosHoras_FechaRegistro]
                    ON [dbo].[RegistrosHoras] ([FechaRegistro])
                    INCLUDE ([UserId]);
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_AspNetUsers_IsActive'
                  AND object_id = OBJECT_ID(N'[dbo].[AspNetUsers]')
            )
            BEGIN
                CREATE INDEX [IX_AspNetUsers_IsActive]
                    ON [dbo].[AspNetUsers] ([IsActive]);
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_NotificacionesEnviadas_Created'
                  AND object_id = OBJECT_ID(N'[dbo].[NotificacionesEnviadas]')
            )
            BEGIN
                CREATE INDEX [IX_NotificacionesEnviadas_Created]
                    ON [dbo].[NotificacionesEnviadas] ([Created]);
            END
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_NotificacionesEnviadas_Created'
                  AND object_id = OBJECT_ID(N'[dbo].[NotificacionesEnviadas]')
            )
            BEGIN
                DROP INDEX [IX_NotificacionesEnviadas_Created] ON [dbo].[NotificacionesEnviadas];
            END
            """);

        migrationBuilder.Sql("""
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_AspNetUsers_IsActive'
                  AND object_id = OBJECT_ID(N'[dbo].[AspNetUsers]')
            )
            BEGIN
                DROP INDEX [IX_AspNetUsers_IsActive] ON [dbo].[AspNetUsers];
            END
            """);

        migrationBuilder.Sql("""
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_RegistrosHoras_FechaRegistro'
                  AND object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
            )
            BEGIN
                DROP INDEX [IX_RegistrosHoras_FechaRegistro] ON [dbo].[RegistrosHoras];
            END
            """);

        migrationBuilder.Sql("""
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_RegistrosHoras_UserId_FechaRegistro_Cliente_Proyecto'
                  AND object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
            )
            BEGIN
                DROP INDEX [IX_RegistrosHoras_UserId_FechaRegistro_Cliente_Proyecto] ON [dbo].[RegistrosHoras];
            END
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_RegistrosHoras_UserId_FechaRegistro'
                  AND object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
            )
            BEGIN
                CREATE UNIQUE INDEX [IX_RegistrosHoras_UserId_FechaRegistro]
                    ON [dbo].[RegistrosHoras] ([UserId], [FechaRegistro]);
            END
            """);
    }
}
