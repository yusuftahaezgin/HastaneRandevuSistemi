using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HastaneRandevuSistemi.Migrations
{
    public partial class HastaneSaatleriVeDoktorHizmetAdi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HizmetAdi",
                table: "Calisanlar",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HastaneCalismaSaatleri",
                columns: table => new
                {
                    Id = table.Column<int>(
                            type: "integer",
                            nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),

                    SubeId = table.Column<int>(
                        type: "integer",
                        nullable: false),

                    Gun = table.Column<int>(
                        type: "integer",
                        nullable: false),

                    AcikMi = table.Column<bool>(
                        type: "boolean",
                        nullable: false),

                    BaslangicDakika = table.Column<int>(
                        type: "integer",
                        nullable: false),

                    BitisDakika = table.Column<int>(
                        type: "integer",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_HastaneCalismaSaatleri",
                        x => x.Id);

                    table.CheckConstraint(
                        "CK_HastaneCalismaSaatleri_Gun",
                        "\"Gun\" BETWEEN 0 AND 6");

                    table.CheckConstraint(
                        "CK_HastaneCalismaSaatleri_Saat",
                        "\"BaslangicDakika\" BETWEEN 0 AND 1439 " +
                        "AND \"BitisDakika\" BETWEEN 1 AND 1440 " +
                        "AND \"BitisDakika\" > \"BaslangicDakika\"");

                    table.ForeignKey(
                        name: "FK_HastaneCalismaSaatleri_Subeler_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Subeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HastaneCalismaSaatleri_SubeId_Gun",
                table: "HastaneCalismaSaatleri",
                columns: new[] { "SubeId", "Gun" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO "HastaneCalismaSaatleri"
                    ("SubeId", "Gun", "AcikMi", "BaslangicDakika", "BitisDakika")
                SELECT
                    s."Id",
                    g.gun,
                    g.gun <> 0,
                    0,
                    1440
                FROM "Subeler" AS s
                CROSS JOIN generate_series(0, 6) AS g(gun)
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "HastaneCalismaSaatleri" AS h
                    WHERE h."SubeId" = s."Id"
                      AND h."Gun" = g.gun
                );

                UPDATE "Calisanlar"
                SET "HizmetAdi" = 'Doktor muayenesi'
                WHERE "CalisanTuru" = 'Doktor'
                  AND ("HizmetAdi" IS NULL OR BTRIM("HizmetAdi") = '');
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HastaneCalismaSaatleri");

            migrationBuilder.DropColumn(
                name: "HizmetAdi",
                table: "Calisanlar");
        }
    }
}