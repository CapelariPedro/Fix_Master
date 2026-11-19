using FixMaster.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FixMaster.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261105220000_Inicial")]
public sealed class Inicial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Usuarios",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Username = table.Column<string>(
                    type: "TEXT",
                    maxLength: 50,
                    nullable: false,
                    collation: "NOCASE"),
                SenhaHash = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Usuarios", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Usuarios_Username",
            table: "Usuarios",
            column: "Username",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "Usuarios");
}
