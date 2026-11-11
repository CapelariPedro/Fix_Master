using FixMaster.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FixMaster.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261110160000_CepCliente")]
public sealed class CepCliente : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Cep",
            table: "Clientes",
            type: "TEXT",
            maxLength: 8,
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(name: "Cep", table: "Clientes");
}
