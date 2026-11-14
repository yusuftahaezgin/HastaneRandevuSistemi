using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HastaneRandevuSistemi.Migrations
{
    public partial class PoliklinikSubeBaglantisi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubeId",
                table: "Poliklinikler",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Poliklinikler_SubeId",
                table: "Poliklinikler",
                column: "SubeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Poliklinikler_Subeler_SubeId",
                table: "Poliklinikler",
                column: "SubeId",
                principalTable: "Subeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Poliklinikler_Subeler_SubeId",
                table: "Poliklinikler");

            migrationBuilder.DropIndex(
                name: "IX_Poliklinikler_SubeId",
                table: "Poliklinikler");

            migrationBuilder.DropColumn(
                name: "SubeId",
                table: "Poliklinikler");
        }
    }
}