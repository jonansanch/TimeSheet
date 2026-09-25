using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPG.Timesheet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SincronizarSnapshotConModeloActual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegistrosHoras_FechaRegistro",
                table: "RegistrosHoras");

            migrationBuilder.DropColumn(
                name: "HoraEntrada",
                table: "RegistrosHoras");

            migrationBuilder.DropColumn(
                name: "HoraSalida",
                table: "RegistrosHoras");

            migrationBuilder.DropColumn(
                name: "Turno",
                table: "RegistrosHoras");

            migrationBuilder.RenameColumn(
                name: "Proyecto",
                table: "RegistrosHoras",
                newName: "ProyectoNombre");

            migrationBuilder.RenameColumn(
                name: "Cliente",
                table: "RegistrosHoras",
                newName: "ClienteNombre");

            migrationBuilder.AddColumn<string>(
                name: "ClienteNombre",
                table: "SolicitudesExcepcion",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "SolicitudesExcepcion",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraEntrada1",
                table: "SolicitudesExcepcion",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraEntrada2",
                table: "SolicitudesExcepcion",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraEntrada3",
                table: "SolicitudesExcepcion",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraSalida1",
                table: "SolicitudesExcepcion",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraSalida2",
                table: "SolicitudesExcepcion",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraSalida3",
                table: "SolicitudesExcepcion",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Lugar",
                table: "SolicitudesExcepcion",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Modalidad",
                table: "SolicitudesExcepcion",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProyectoId",
                table: "SolicitudesExcepcion",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProyectoNombre",
                table: "SolicitudesExcepcion",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Recurso",
                table: "SolicitudesExcepcion",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComentarioRechazo",
                table: "RegistrosHoras",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "RegistrosHoras",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstadoPrevioAlRechazo",
                table: "RegistrosHoras",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraEntrada1",
                table: "RegistrosHoras",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraEntrada2",
                table: "RegistrosHoras",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraEntrada3",
                table: "RegistrosHoras",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraSalida1",
                table: "RegistrosHoras",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraSalida2",
                table: "RegistrosHoras",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraSalida3",
                table: "RegistrosHoras",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProyectoId",
                table: "RegistrosHoras",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SupervisorUserId",
                table: "Proyectos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Valor",
                table: "ParametrosSistema",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<int>(
                name: "PuestoId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupervisorUserId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AprobacionesRegistro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistroHorasId = table.Column<int>(type: "int", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nivel = table.Column<int>(type: "int", nullable: true),
                    ActorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AprobacionesRegistro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AprobacionesRegistro_RegistrosHoras_RegistroHorasId",
                        column: x => x.RegistroHorasId,
                        principalTable: "RegistrosHoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParametrosRestriccionDia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiaDelaSemana = table.Column<int>(type: "int", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParametrosRestriccionDia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReglasVentanaRetroactividad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Rol = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Dias = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasVentanaRetroactividad", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportesUsuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ComentarioRespuesta = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RespondidoPorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesUsuario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupervisoresPuesto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PuestoId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    SupervisorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupervisoresPuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupervisoresPuesto_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupervisoresPuesto_Empleados_PuestoId",
                        column: x => x.PuestoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TerminosDescripcion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Termino = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TerminoNormalizado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Regla = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Severidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Sugerencia = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminosDescripcion", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesExcepcion_ProyectoId",
                table: "SolicitudesExcepcion",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosHoras_Estado_FechaRegistro",
                table: "RegistrosHoras",
                columns: new[] { "Estado", "FechaRegistro" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosHoras_FechaRegistro",
                table: "RegistrosHoras",
                column: "FechaRegistro")
                .Annotation("SqlServer:Include", new[] { "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosHoras_ProyectoId",
                table: "RegistrosHoras",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosHoras_UserId_FechaRegistro_ProyectoId",
                table: "RegistrosHoras",
                columns: new[] { "UserId", "FechaRegistro", "ProyectoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificacionesEnviadas_Created",
                table: "NotificacionesEnviadas",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsActive",
                table: "AspNetUsers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PuestoId",
                table: "AspNetUsers",
                column: "PuestoId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SupervisorUserId",
                table: "AspNetUsers",
                column: "SupervisorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AprobacionesRegistro_RegistroHorasId",
                table: "AprobacionesRegistro",
                column: "RegistroHorasId");

            migrationBuilder.CreateIndex(
                name: "IX_ParametrosRestriccionDia_Dia_Rol",
                table: "ParametrosRestriccionDia",
                columns: new[] { "DiaDelaSemana", "Rol" },
                unique: true,
                filter: "[Rol] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ParametrosRestriccionDia_Dia_UserId",
                table: "ParametrosRestriccionDia",
                columns: new[] { "DiaDelaSemana", "UserId" },
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasVentanaRetroactividad_Rol",
                table: "ReglasVentanaRetroactividad",
                column: "Rol",
                unique: true,
                filter: "[Rol] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasVentanaRetroactividad_UserId",
                table: "ReglasVentanaRetroactividad",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesUsuario_Estado",
                table: "ReportesUsuario",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesUsuario_UserId",
                table: "ReportesUsuario",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupervisoresPuesto_ClienteId",
                table: "SupervisoresPuesto",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_SupervisoresPuesto_PuestoId_ClienteId",
                table: "SupervisoresPuesto",
                columns: new[] { "PuestoId", "ClienteId" },
                unique: true,
                filter: "[ClienteId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SupervisoresPuesto_PuestoId_General",
                table: "SupervisoresPuesto",
                column: "PuestoId",
                unique: true,
                filter: "[ClienteId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TerminosDescripcion_Activo",
                table: "TerminosDescripcion",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_TerminosDescripcion_TerminoNormalizado",
                table: "TerminosDescripcion",
                column: "TerminoNormalizado",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_SupervisorUserId",
                table: "AspNetUsers",
                column: "SupervisorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Empleados_PuestoId",
                table: "AspNetUsers",
                column: "PuestoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosHoras_Proyectos_ProyectoId",
                table: "RegistrosHoras",
                column: "ProyectoId",
                principalTable: "Proyectos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesExcepcion_Proyectos_ProyectoId",
                table: "SolicitudesExcepcion",
                column: "ProyectoId",
                principalTable: "Proyectos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_SupervisorUserId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Empleados_PuestoId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosHoras_Proyectos_ProyectoId",
                table: "RegistrosHoras");

            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesExcepcion_Proyectos_ProyectoId",
                table: "SolicitudesExcepcion");

            migrationBuilder.DropTable(
                name: "AprobacionesRegistro");

            migrationBuilder.DropTable(
                name: "ParametrosRestriccionDia");

            migrationBuilder.DropTable(
                name: "ReglasVentanaRetroactividad");

            migrationBuilder.DropTable(
                name: "ReportesUsuario");

            migrationBuilder.DropTable(
                name: "SupervisoresPuesto");

            migrationBuilder.DropTable(
                name: "TerminosDescripcion");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesExcepcion_ProyectoId",
                table: "SolicitudesExcepcion");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosHoras_Estado_FechaRegistro",
                table: "RegistrosHoras");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosHoras_FechaRegistro",
                table: "RegistrosHoras");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosHoras_ProyectoId",
                table: "RegistrosHoras");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosHoras_UserId_FechaRegistro_ProyectoId",
                table: "RegistrosHoras");

            migrationBuilder.DropIndex(
                name: "IX_NotificacionesEnviadas_Created",
                table: "NotificacionesEnviadas");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IsActive",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PuestoId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SupervisorUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ClienteNombre",
                table: "SolicitudesExcepcion");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "SolicitudesExcepcion");

            migrationBuilder.DropColumn(
                name: "HoraEntrada1",
                table: "SolicitudesExcepcion");

            migrationBuilder.DropColumn(
                name: "HoraEntrada2",
                table: "SolicitudesExcepcion");

            migrationBuilder.DropColumn(
                name: "HoraEntrada3",
                table: "SolicitudesExcepcion");

            migrationBuilder.DropColumn(
                name: "HoraSalida1",
                table: "SolicitudesExcepcion");

            migrationBuilder.DropColumn(
                name: "HoraSalida2",
                table: "SolicitudesExcepcion");

            migrationBuilder.DropColumn(
                name: "HoraSalida3",
                table: "SolicitudesExcepcion");

            migrationBuilder.DropColumn(
                name: "Lugar",
                table: "SolicitudesExcepcion");

            migrationBuilder.DropColumn(
                name: "Modalidad",
                table: "SolicitudesExcepcion");

            migrationBuilder.DropColumn(
                name: "ProyectoId",
                table: "SolicitudesExcepcion");

            migrationBuilder.DropColumn(
                name: "ProyectoNombre",
                table: "SolicitudesExcepcion");

            migrationBuilder.DropColumn(
                name: "Recurso",
                table: "SolicitudesExcepcion");

            migrationBuilder.DropColumn(
                name: "ComentarioRechazo",
                table: "RegistrosHoras");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "RegistrosHoras");

            migrationBuilder.DropColumn(
                name: "EstadoPrevioAlRechazo",
                table: "RegistrosHoras");

            migrationBuilder.DropColumn(
                name: "HoraEntrada1",
                table: "RegistrosHoras");

            migrationBuilder.DropColumn(
                name: "HoraEntrada2",
                table: "RegistrosHoras");

            migrationBuilder.DropColumn(
                name: "HoraEntrada3",
                table: "RegistrosHoras");

            migrationBuilder.DropColumn(
                name: "HoraSalida1",
                table: "RegistrosHoras");

            migrationBuilder.DropColumn(
                name: "HoraSalida2",
                table: "RegistrosHoras");

            migrationBuilder.DropColumn(
                name: "HoraSalida3",
                table: "RegistrosHoras");

            migrationBuilder.DropColumn(
                name: "ProyectoId",
                table: "RegistrosHoras");

            migrationBuilder.DropColumn(
                name: "SupervisorUserId",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "PuestoId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SupervisorUserId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "ProyectoNombre",
                table: "RegistrosHoras",
                newName: "Proyecto");

            migrationBuilder.RenameColumn(
                name: "ClienteNombre",
                table: "RegistrosHoras",
                newName: "Cliente");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraEntrada",
                table: "RegistrosHoras",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraSalida",
                table: "RegistrosHoras",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "Turno",
                table: "RegistrosHoras",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Valor",
                table: "ParametrosSistema",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosHoras_FechaRegistro",
                table: "RegistrosHoras",
                column: "FechaRegistro");
        }
    }
}
