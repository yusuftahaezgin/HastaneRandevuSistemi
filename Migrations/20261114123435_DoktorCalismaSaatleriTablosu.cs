using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HastaneRandevuSistemi.Migrations
{
    /// <inheritdoc />
    public partial class DoktorCalismaSaatleriTablosu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DoktorCalismaSaatleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoktorId = table.Column<int>(type: "integer", nullable: false),
                    Gun = table.Column<int>(type: "integer", nullable: false),
                    BaslangicSaati = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    BitisSaati = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    RandevuSuresiDakika = table.Column<int>(type: "integer", nullable: false),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoktorCalismaSaatleri", x => x.Id);
                    table.CheckConstraint("CK_DoktorCalismaSaatleri_DakikaHassasiyeti", "EXTRACT(SECOND FROM \"BaslangicSaati\") = 0 AND EXTRACT(SECOND FROM \"BitisSaati\") = 0");
                    table.CheckConstraint("CK_DoktorCalismaSaatleri_Gun", "\"Gun\" BETWEEN 0 AND 6");
                    table.CheckConstraint("CK_DoktorCalismaSaatleri_MinimumAralik", "EXTRACT(EPOCH FROM (\"BitisSaati\" - \"BaslangicSaati\")) >= \"RandevuSuresiDakika\" * 60");
                    table.CheckConstraint("CK_DoktorCalismaSaatleri_RandevuSuresi", "\"RandevuSuresiDakika\" BETWEEN 5 AND 120");
                    table.CheckConstraint("CK_DoktorCalismaSaatleri_SaatAraligi", "\"BitisSaati\" > \"BaslangicSaati\"");
                    table.ForeignKey(
                        name: "FK_DoktorCalismaSaatleri_Calisanlar_DoktorId",
                        column: x => x.DoktorId,
                        principalTable: "Calisanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoktorCalismaSaatleri_DoktorId_Gun",
                table: "DoktorCalismaSaatleri",
                columns: new[] { "DoktorId", "Gun" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoktorCalismaSaatleri");
        }
    }
}
