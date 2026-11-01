using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HastaneRandevuSistemi.Migrations
{
    /// <inheritdoc />
    public partial class DoktorKullaniciBaglantisi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KullaniciId",
                table: "Doktorlar",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Doktorlar",
                keyColumn: "Id",
                keyValue: 1,
                column: "KullaniciId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Doktorlar",
                keyColumn: "Id",
                keyValue: 2,
                column: "KullaniciId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Doktorlar",
                keyColumn: "Id",
                keyValue: 3,
                column: "KullaniciId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Doktorlar",
                keyColumn: "Id",
                keyValue: 4,
                column: "KullaniciId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Doktorlar",
                keyColumn: "Id",
                keyValue: 5,
                column: "KullaniciId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Doktorlar",
                keyColumn: "Id",
                keyValue: 6,
                column: "KullaniciId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Doktorlar",
                keyColumn: "Id",
                keyValue: 7,
                column: "KullaniciId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Doktorlar",
                keyColumn: "Id",
                keyValue: 8,
                column: "KullaniciId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Doktorlar",
                keyColumn: "Id",
                keyValue: 9,
                column: "KullaniciId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Doktorlar",
                keyColumn: "Id",
                keyValue: 10,
                column: "KullaniciId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Doktorlar",
                keyColumn: "Id",
                keyValue: 11,
                column: "KullaniciId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Doktorlar",
                keyColumn: "Id",
                keyValue: 12,
                column: "KullaniciId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Doktorlar_KullaniciId",
                table: "Doktorlar",
                column: "KullaniciId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Doktorlar_AspNetUsers_KullaniciId",
                table: "Doktorlar",
                column: "KullaniciId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doktorlar_AspNetUsers_KullaniciId",
                table: "Doktorlar");

            migrationBuilder.DropIndex(
                name: "IX_Doktorlar_KullaniciId",
                table: "Doktorlar");

            migrationBuilder.DropColumn(
                name: "KullaniciId",
                table: "Doktorlar");
        }
    }
}
