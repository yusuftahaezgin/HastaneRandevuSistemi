using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HastaneRandevuSistemi.Migrations
{
    /// <inheritdoc />
    public partial class DoktorlariEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Doktorlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Unvan = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Ad = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Soyad = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    PoliklinikId = table.Column<int>(type: "integer", nullable: false),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doktorlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doktorlar_Poliklinikler_PoliklinikId",
                        column: x => x.PoliklinikId,
                        principalTable: "Poliklinikler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Doktorlar",
                columns: new[] { "Id", "Ad", "AktifMi", "PoliklinikId", "Soyad", "Unvan" },
                values: new object[,]
                {
                    { 1, "Deniz", true, 1, "Kaya", "Uzm. Dr." },
                    { 2, "Ece", true, 2, "Yılmaz", "Uzm. Dr." },
                    { 3, "Mert", true, 3, "Demir", "Uzm. Dr." },
                    { 4, "Selin", true, 4, "Arslan", "Uzm. Dr." },
                    { 5, "Can", true, 5, "Yıldız", "Uzm. Dr." },
                    { 6, "Elif", true, 6, "Aydın", "Uzm. Dr." },
                    { 7, "Burak", true, 7, "Şahin", "Uzm. Dr." },
                    { 8, "Derya", true, 8, "Koç", "Uzm. Dr." },
                    { 9, "Zeynep", true, 9, "Çelik", "Uzm. Dr." },
                    { 10, "Emre", true, 10, "Kurt", "Uzm. Dr." },
                    { 11, "Ceren", true, 11, "Özkan", "Uzm. Dr." },
                    { 12, "Kerem", true, 12, "Aslan", "Uzm. Dr." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Doktorlar_PoliklinikId",
                table: "Doktorlar",
                column: "PoliklinikId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Doktorlar");
        }
    }
}
