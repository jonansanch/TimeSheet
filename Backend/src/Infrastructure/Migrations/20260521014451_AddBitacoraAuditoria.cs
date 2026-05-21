using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPG.Timesheet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBitacoraAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BitacoraAuditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoEvento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ActorEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EntidadAfectada = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntidadId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BitacoraAuditoria", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BitacoraAuditoria_ActorId",
                table: "BitacoraAuditoria",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_BitacoraAuditoria_Timestamp",
                table: "BitacoraAuditoria",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_BitacoraAuditoria_TipoEvento",
                table: "BitacoraAuditoria",
                column: "TipoEvento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BitacoraAuditoria");
        }
    }
}
