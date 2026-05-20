using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPG.Timesheet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificacionEnviada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificacionesEnviadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FechaReferencia = table.Column<DateOnly>(type: "date", nullable: false),
                    DiasAcumulados = table.Column<int>(type: "int", nullable: false),
                    Exitoso = table.Column<bool>(type: "bit", nullable: false),
                    ErrorDetalle = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificacionesEnviadas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificacionesEnviadas_UserId_Created",
                table: "NotificacionesEnviadas",
                columns: new[] { "UserId", "Created" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificacionesEnviadas");
        }
    }
}
