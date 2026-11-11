using FixMaster.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FixMaster.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261110150000_Clientes")]
public sealed class Clientes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Clientes",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Nome = table.Column<string>(
                    type: "TEXT",
                    maxLength: 150,
                    nullable: false,
                    collation: "NOCASE"),
                Documento = table.Column<string>(
                    type: "TEXT",
                    maxLength: 14,
                    nullable: false),
                Telefone = table.Column<string>(
                    type: "TEXT",
                    maxLength: 30,
                    nullable: false),
                Endereco = table.Column<string>(
                    type: "TEXT",
                    maxLength: 250,
                    nullable: false),
                EhEmpresa = table.Column<bool>(type: "INTEGER", nullable: false),
                Ativo = table.Column<bool>(
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: true)
            },
            constraints: table => table.PrimaryKey("PK_Clientes", cliente => cliente.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Clientes_Documento",
            table: "Clientes",
            column: "Documento",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "Clientes");
}
