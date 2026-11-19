using FixMaster.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FixMaster.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261111134500_Estoque")]
public sealed class Estoque : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ItensEstoque",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Codigo = table.Column<string>(
                    type: "TEXT",
                    maxLength: 40,
                    nullable: false,
                    collation: "NOCASE"),
                Nome = table.Column<string>(
                    type: "TEXT",
                    maxLength: 150,
                    nullable: false,
                    collation: "NOCASE"),
                Descricao = table.Column<string>(
                    type: "TEXT",
                    maxLength: 500,
                    nullable: false,
                    defaultValue: ""),
                Categoria = table.Column<string>(
                    type: "TEXT",
                    maxLength: 80,
                    nullable: false,
                    collation: "NOCASE"),
                Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                PrecoUnitario = table.Column<decimal>(type: "TEXT", nullable: false),
                Prateleira = table.Column<string>(
                    type: "TEXT",
                    maxLength: 1,
                    nullable: false),
                Posicao = table.Column<int>(type: "INTEGER", nullable: false),
                Ativo = table.Column<bool>(
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ItensEstoque", item => item.Id);
                table.CheckConstraint(
                    "CK_ItensEstoque_Quantidade",
                    "\"Quantidade\" >= 0");
                table.CheckConstraint(
                    "CK_ItensEstoque_Prateleira",
                    "length(\"Prateleira\") = 1 AND \"Prateleira\" >= 'A' AND \"Prateleira\" <= 'X'");
                table.CheckConstraint(
                    "CK_ItensEstoque_Posicao",
                    "\"Posicao\" >= 1 AND \"Posicao\" <= 8");
            });

        migrationBuilder.CreateIndex(
            name: "IX_ItensEstoque_Codigo",
            table: "ItensEstoque",
            column: "Codigo",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ItensEstoque_Prateleira_Posicao",
            table: "ItensEstoque",
            columns: new[] { "Prateleira", "Posicao" },
            unique: true,
            filter: "\"Ativo\" = 1");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "ItensEstoque");
}
