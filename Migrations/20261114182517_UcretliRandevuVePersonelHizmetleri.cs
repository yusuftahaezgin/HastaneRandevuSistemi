using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HastaneRandevuSistemi.Migrations
{
    /// <inheritdoc />
    public partial class UcretliRandevuVePersonelHizmetleri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RandevuUcreti",
                table: "Calisanlar",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PersonelCalismaSaatleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    Gun = table.Column<int>(type: "integer", nullable: false),
                    Baslangic = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Bitis = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonelCalismaSaatleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonelCalismaSaatleri_Calisanlar_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Calisanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonelHizmetleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    Ad = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SureDakika = table.Column<int>(type: "integer", nullable: false),
                    Ucret = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonelHizmetleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonelHizmetleri_Calisanlar_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Calisanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Randevular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HastaId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CalisanId = table.Column<int>(type: "integer", nullable: false),
                    PersonelHizmetiId = table.Column<int>(type: "integer", nullable: true),
                    PoliklinikId = table.Column<int>(type: "integer", nullable: false),
                    Tarih = table.Column<DateOnly>(type: "date", nullable: false),
                    Baslangic = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Bitis = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    SureDakika = table.Column<int>(type: "integer", nullable: false),
                    Ucret = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    IslemAdi = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CalisanAdi = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    HastaneAdi = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PoliklinikAdi = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    OlusturmaZamaniUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Randevular", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Randevular_AspNetUsers_HastaId",
                        column: x => x.HastaId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Randevular_Calisanlar_CalisanId",
                        column: x => x.CalisanId,
                        principalTable: "Calisanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Randevular_PersonelHizmetleri_PersonelHizmetiId",
                        column: x => x.PersonelHizmetiId,
                        principalTable: "PersonelHizmetleri",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Calisanlar",
                keyColumn: "Id",
                keyValue: 1,
                column: "RandevuUcreti",
                value: null);

            migrationBuilder.UpdateData(
                table: "Calisanlar",
                keyColumn: "Id",
                keyValue: 2,
                column: "RandevuUcreti",
                value: null);

            migrationBuilder.UpdateData(
                table: "Calisanlar",
                keyColumn: "Id",
                keyValue: 3,
                column: "RandevuUcreti",
                value: null);

            migrationBuilder.UpdateData(
                table: "Calisanlar",
                keyColumn: "Id",
                keyValue: 4,
                column: "RandevuUcreti",
                value: null);

            migrationBuilder.UpdateData(
                table: "Calisanlar",
                keyColumn: "Id",
                keyValue: 5,
                column: "RandevuUcreti",
                value: null);

            migrationBuilder.UpdateData(
                table: "Calisanlar",
                keyColumn: "Id",
                keyValue: 6,
                column: "RandevuUcreti",
                value: null);

            migrationBuilder.UpdateData(
                table: "Calisanlar",
                keyColumn: "Id",
                keyValue: 7,
                column: "RandevuUcreti",
                value: null);

            migrationBuilder.UpdateData(
                table: "Calisanlar",
                keyColumn: "Id",
                keyValue: 8,
                column: "RandevuUcreti",
                value: null);

            migrationBuilder.UpdateData(
                table: "Calisanlar",
                keyColumn: "Id",
                keyValue: 9,
                column: "RandevuUcreti",
                value: null);

            migrationBuilder.UpdateData(
                table: "Calisanlar",
                keyColumn: "Id",
                keyValue: 10,
                column: "RandevuUcreti",
                value: null);

            migrationBuilder.UpdateData(
                table: "Calisanlar",
                keyColumn: "Id",
                keyValue: 11,
                column: "RandevuUcreti",
                value: null);

            migrationBuilder.UpdateData(
                table: "Calisanlar",
                keyColumn: "Id",
                keyValue: 12,
                column: "RandevuUcreti",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_PersonelCalismaSaatleri_PersonelId",
                table: "PersonelCalismaSaatleri",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonelHizmetleri_PersonelId",
                table: "PersonelHizmetleri",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_CalisanId_Tarih",
                table: "Randevular",
                columns: new[] { "CalisanId", "Tarih" });

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_HastaId_Tarih",
                table: "Randevular",
                columns: new[] { "HastaId", "Tarih" });

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_PersonelHizmetiId",
                table: "Randevular",
                column: "PersonelHizmetiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonelCalismaSaatleri");

            migrationBuilder.DropTable(
                name: "Randevular");

            migrationBuilder.DropTable(
                name: "PersonelHizmetleri");

            migrationBuilder.DropColumn(
                name: "RandevuUcreti",
                table: "Calisanlar");
        }
    }
}
