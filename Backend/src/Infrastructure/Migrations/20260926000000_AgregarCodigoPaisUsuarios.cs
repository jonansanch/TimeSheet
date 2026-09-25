using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPG.Timesheet.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260926000000_AgregarCodigoPaisUsuarios")]
public sealed class AgregarCodigoPaisUsuarios : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CodigoPais",
            table: "AspNetUsers",
            type: "nvarchar(2)",
            maxLength: 2,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "CodigoPais", table: "AspNetUsers");
    }
}
