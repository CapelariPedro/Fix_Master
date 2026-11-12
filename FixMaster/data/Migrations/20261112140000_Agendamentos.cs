using System;
using FixMaster.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FixMaster.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261112140000_Agendamentos")]
public sealed class Agendamentos : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Agendamentos",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ClienteId = table.Column<int>(type: "INTEGER", nullable: false),
                DataHoraInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                DuracaoMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                DataHoraFim = table.Column<DateTime>(type: "TEXT", nullable: false),
                DescricaoServico = table.Column<string>(
                    type: "TEXT",
                    maxLength: 500,
                    nullable: false),
                Status = table.Column<int>(
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Agendamentos", agendamento => agendamento.Id);
                table.CheckConstraint(
                    "CK_Agendamentos_DuracaoMinutos",
                    "\"DuracaoMinutos\" > 0 AND \"DuracaoMinutos\" <= 1440");
                table.CheckConstraint(
                    "CK_Agendamentos_Status",
                    "\"Status\" >= 0 AND \"Status\" <= 3");
                table.ForeignKey(
                    name: "FK_Agendamentos_Clientes_ClienteId",
                    column: agendamento => agendamento.ClienteId,
                    principalTable: "Clientes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Agendamentos_ClienteId",
            table: "Agendamentos",
            column: "ClienteId");

        migrationBuilder.CreateIndex(
            name: "IX_Agendamentos_DataHoraInicio",
            table: "Agendamentos",
            column: "DataHoraInicio");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "Agendamentos");
}
