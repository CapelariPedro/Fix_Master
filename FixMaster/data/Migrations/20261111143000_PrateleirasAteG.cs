using FixMaster.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FixMaster.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261111143000_PrateleirasAteG")]
public sealed class PrateleirasAteG : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE TABLE "__EstoquePrateleirasOriginais" (
                "Id" INTEGER NOT NULL PRIMARY KEY,
                "Prateleira" TEXT NOT NULL,
                "Posicao" INTEGER NOT NULL
            );

            INSERT INTO "__EstoquePrateleirasOriginais" ("Id", "Prateleira", "Posicao")
            SELECT "Id", "Prateleira", "Posicao"
            FROM "ItensEstoque"
            WHERE "Prateleira" > 'G';

            CREATE TEMP TABLE "__EstoqueNovasLocalizacoes" (
                "Id" INTEGER NOT NULL PRIMARY KEY,
                "Prateleira" TEXT NOT NULL,
                "Posicao" INTEGER NOT NULL
            );

            WITH RECURSIVE
            "Espacos" ("Prateleira", "Posicao") AS (
                SELECT 'A', 1
                UNION ALL
                SELECT
                    CASE WHEN "Posicao" = 8
                        THEN char(unicode("Prateleira") + 1)
                        ELSE "Prateleira"
                    END,
                    CASE WHEN "Posicao" = 8 THEN 1 ELSE "Posicao" + 1 END
                FROM "Espacos"
                WHERE "Prateleira" < 'G'
                   OR ("Prateleira" = 'G' AND "Posicao" < 8)
            ),
            "ItensParaMover" AS (
                SELECT "Id", ROW_NUMBER() OVER (ORDER BY "Id") AS "Numero"
                FROM "ItensEstoque"
                WHERE "Ativo" = 1 AND "Prateleira" > 'G'
            ),
            "EspacosLivres" AS (
                SELECT
                    "Espacos"."Prateleira",
                    "Espacos"."Posicao",
                    ROW_NUMBER() OVER (
                        ORDER BY "Espacos"."Prateleira", "Espacos"."Posicao"
                    ) AS "Numero"
                FROM "Espacos"
                LEFT JOIN "ItensEstoque" AS "Existente"
                    ON "Existente"."Ativo" = 1
                    AND "Existente"."Prateleira" = "Espacos"."Prateleira"
                    AND "Existente"."Posicao" = "Espacos"."Posicao"
                WHERE "Existente"."Id" IS NULL
            )
            INSERT INTO "__EstoqueNovasLocalizacoes" ("Id", "Prateleira", "Posicao")
            SELECT "ItensParaMover"."Id",
                   "EspacosLivres"."Prateleira",
                   "EspacosLivres"."Posicao"
            FROM "ItensParaMover"
            LEFT JOIN "EspacosLivres"
                ON "EspacosLivres"."Numero" = "ItensParaMover"."Numero";

            INSERT INTO "__EstoqueNovasLocalizacoes" ("Id", "Prateleira", "Posicao")
            SELECT "Id", 'A', 1
            FROM "ItensEstoque"
            WHERE "Ativo" = 0 AND "Prateleira" > 'G';

            CREATE TABLE "ItensEstoque_AtoG" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_ItensEstoque_AtoG" PRIMARY KEY AUTOINCREMENT,
                "Codigo" TEXT COLLATE NOCASE NOT NULL,
                "Nome" TEXT COLLATE NOCASE NOT NULL,
                "Descricao" TEXT NOT NULL DEFAULT '',
                "Categoria" TEXT COLLATE NOCASE NOT NULL,
                "Quantidade" INTEGER NOT NULL,
                "PrecoUnitario" TEXT NOT NULL,
                "Prateleira" TEXT NOT NULL,
                "Posicao" INTEGER NOT NULL,
                "Ativo" INTEGER NOT NULL DEFAULT 1,
                CONSTRAINT "CK_ItensEstoque_Quantidade" CHECK ("Quantidade" >= 0),
                CONSTRAINT "CK_ItensEstoque_Prateleira"
                    CHECK (length("Prateleira") = 1 AND "Prateleira" >= 'A' AND "Prateleira" <= 'G'),
                CONSTRAINT "CK_ItensEstoque_Posicao" CHECK ("Posicao" >= 1 AND "Posicao" <= 8)
            );

            INSERT INTO "ItensEstoque_AtoG" (
                "Id", "Codigo", "Nome", "Descricao", "Categoria", "Quantidade",
                "PrecoUnitario", "Prateleira", "Posicao", "Ativo"
            )
            SELECT
                "Item"."Id",
                "Item"."Codigo",
                "Item"."Nome",
                "Item"."Descricao",
                "Item"."Categoria",
                "Item"."Quantidade",
                "Item"."PrecoUnitario",
                COALESCE("NovaLocalizacao"."Prateleira", "Item"."Prateleira"),
                COALESCE("NovaLocalizacao"."Posicao", "Item"."Posicao"),
                "Item"."Ativo"
            FROM "ItensEstoque" AS "Item"
            LEFT JOIN "__EstoqueNovasLocalizacoes" AS "NovaLocalizacao"
                ON "NovaLocalizacao"."Id" = "Item"."Id";

            DROP TABLE "ItensEstoque";
            ALTER TABLE "ItensEstoque_AtoG" RENAME TO "ItensEstoque";
            DROP TABLE "__EstoqueNovasLocalizacoes";

            CREATE UNIQUE INDEX "IX_ItensEstoque_Codigo"
                ON "ItensEstoque" ("Codigo");
            CREATE UNIQUE INDEX "IX_ItensEstoque_Prateleira_Posicao"
                ON "ItensEstoque" ("Prateleira", "Posicao")
                WHERE "Ativo" = 1;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE TABLE "ItensEstoque_AtoX" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_ItensEstoque_AtoX" PRIMARY KEY AUTOINCREMENT,
                "Codigo" TEXT COLLATE NOCASE NOT NULL,
                "Nome" TEXT COLLATE NOCASE NOT NULL,
                "Descricao" TEXT NOT NULL DEFAULT '',
                "Categoria" TEXT COLLATE NOCASE NOT NULL,
                "Quantidade" INTEGER NOT NULL,
                "PrecoUnitario" TEXT NOT NULL,
                "Prateleira" TEXT NOT NULL,
                "Posicao" INTEGER NOT NULL,
                "Ativo" INTEGER NOT NULL DEFAULT 1,
                CONSTRAINT "CK_ItensEstoque_Quantidade" CHECK ("Quantidade" >= 0),
                CONSTRAINT "CK_ItensEstoque_Prateleira"
                    CHECK (length("Prateleira") = 1 AND "Prateleira" >= 'A' AND "Prateleira" <= 'X'),
                CONSTRAINT "CK_ItensEstoque_Posicao" CHECK ("Posicao" >= 1 AND "Posicao" <= 8)
            );

            INSERT INTO "ItensEstoque_AtoX" (
                "Id", "Codigo", "Nome", "Descricao", "Categoria", "Quantidade",
                "PrecoUnitario", "Prateleira", "Posicao", "Ativo"
            )
            SELECT
                "Item"."Id",
                "Item"."Codigo",
                "Item"."Nome",
                "Item"."Descricao",
                "Item"."Categoria",
                "Item"."Quantidade",
                "Item"."PrecoUnitario",
                COALESCE("Original"."Prateleira", "Item"."Prateleira"),
                COALESCE("Original"."Posicao", "Item"."Posicao"),
                "Item"."Ativo"
            FROM "ItensEstoque" AS "Item"
            LEFT JOIN "__EstoquePrateleirasOriginais" AS "Original"
                ON "Original"."Id" = "Item"."Id";

            DROP TABLE "ItensEstoque";
            ALTER TABLE "ItensEstoque_AtoX" RENAME TO "ItensEstoque";
            DROP TABLE "__EstoquePrateleirasOriginais";

            CREATE UNIQUE INDEX "IX_ItensEstoque_Codigo"
                ON "ItensEstoque" ("Codigo");
            CREATE UNIQUE INDEX "IX_ItensEstoque_Prateleira_Posicao"
                ON "ItensEstoque" ("Prateleira", "Posicao")
                WHERE "Ativo" = 1;
            """);
    }
}
