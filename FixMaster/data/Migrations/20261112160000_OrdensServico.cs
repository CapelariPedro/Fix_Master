using System;
using FixMaster.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FixMaster.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261112160000_OrdensServico")]
public sealed class OrdensServico : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "OrdensServico",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                AgendamentoId = table.Column<int>(type: "INTEGER", nullable: false),
                RelatorioTecnico = table.Column<string>(
                    type: "TEXT",
                    maxLength: 5000,
                    nullable: false),
                MaoDeObra = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                DataEmissao = table.Column<DateTime>(type: "TEXT", nullable: false),
                DataConclusao = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrdensServico", ordem => ordem.Id);
                table.CheckConstraint("CK_OrdensServico_MaoDeObra", "\"MaoDeObra\" >= 0");
                table.CheckConstraint("CK_OrdensServico_Status", "\"Status\" >= 0 AND \"Status\" <= 1");
                table.ForeignKey(
                    name: "FK_OrdensServico_Agendamentos_AgendamentoId",
                    column: ordem => ordem.AgendamentoId,
                    principalTable: "Agendamentos",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ItensOS",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                OrdemServicoId = table.Column<int>(type: "INTEGER", nullable: false),
                ItemEstoqueId = table.Column<int>(type: "INTEGER", nullable: false),
                QuantidadeUtilizada = table.Column<int>(type: "INTEGER", nullable: false),
                ValorUnitario = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ItensOS", item => item.Id);
                table.CheckConstraint("CK_ItensOS_QuantidadeUtilizada", "\"QuantidadeUtilizada\" > 0");
                table.CheckConstraint("CK_ItensOS_ValorUnitario", "\"ValorUnitario\" >= 0");
                table.ForeignKey(
                    name: "FK_ItensOS_ItensEstoque_ItemEstoqueId",
                    column: item => item.ItemEstoqueId,
                    principalTable: "ItensEstoque",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ItensOS_OrdensServico_OrdemServicoId",
                    column: item => item.OrdemServicoId,
                    principalTable: "OrdensServico",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_OrdensServico_AgendamentoId",
            table: "OrdensServico",
            column: "AgendamentoId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ItensOS_ItemEstoqueId",
            table: "ItensOS",
            column: "ItemEstoqueId");

        migrationBuilder.CreateIndex(
            name: "IX_ItensOS_OrdemServicoId_ItemEstoqueId",
            table: "ItensOS",
            columns: new[] { "OrdemServicoId", "ItemEstoqueId" },
            unique: true);

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ItensOS");
        migrationBuilder.DropTable(name: "OrdensServico");
    }
}
