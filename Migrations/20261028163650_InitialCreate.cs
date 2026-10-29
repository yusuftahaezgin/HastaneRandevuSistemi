using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HastaneRandevuSistemi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Poliklinikler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Aciklama = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Poliklinikler", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Poliklinikler",
                columns: new[] { "Id", "Aciklama", "Ad", "AktifMi" },
                values: new object[,]
                {
                    { 1, "İç hastalıklarının değerlendirilmesi.", "Dahiliye", true },
                    { 2, "Kalp ve dolaşım sistemi hastalıkları.", "Kardiyoloji", true },
                    { 3, "Çocuk sağlığı ve hastalıklarının değerlendirilmesi.", "Çocuk Hastalıkları", true },
                    { 4, "Cilt, saç ve tırnak hastalıkları.", "Dermatoloji", true },
                    { 5, "Kemik, eklem ve kas-iskelet sistemi sağlığı.", "Ortopedi ve Travmatoloji", true },
                    { 6, "Göz ve görme sağlığı.", "Göz Hastalıkları", true },
                    { 7, "Sinir sistemi hastalıklarının değerlendirilmesi.", "Nöroloji", true },
                    { 8, "Kulak, burun ve boğaz hastalıkları.", "Kulak Burun Boğaz", true },
                    { 9, "Kadın sağlığı ve gebelik takibi.", "Kadın Hastalıkları ve Doğum", true },
                    { 10, "İdrar yolları ve ilgili hastalıklar.", "Üroloji", true },
                    { 11, "Cerrahi değerlendirme ve tedavi süreçleri.", "Genel Cerrahi", true },
                    { 12, "Ruh sağlığı değerlendirmeleri.", "Psikiyatri", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Poliklinikler");
        }
    }
}
