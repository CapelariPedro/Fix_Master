using System;
using FixMaster.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FixMaster.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261112210000_AtividadesRecentes")]
public sealed class AtividadesRecentes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AtividadesRecentes",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                DataHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                Tipo = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                ReferenciaId = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
                table.PrimaryKey("PK_AtividadesRecentes", atividade => atividade.Id));

        migrationBuilder.CreateIndex(
            name: "IX_AtividadesRecentes_DataHora",
            table: "AtividadesRecentes",
            column: "DataHora");

        migrationBuilder.CreateIndex(
            name: "IX_AtividadesRecentes_Tipo_ReferenciaId",
            table: "AtividadesRecentes",
            columns: new[] { "Tipo", "ReferenciaId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "AtividadesRecentes");
}
