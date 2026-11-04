using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HastaneRandevuSistemi.Migrations
{
    public partial class CalisanKalitimi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eski isimleri kullanan ilişkileri kaldır.
            migrationBuilder.DropForeignKey(
                name: "FK_Doktorlar_AspNetUsers_KullaniciId",
                table: "Doktorlar");

            migrationBuilder.DropForeignKey(
                name: "FK_Doktorlar_Poliklinikler_PoliklinikId",
                table: "Doktorlar");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Doktorlar",
                table: "Doktorlar");

            // Tabloyu mevcut kayıtlarıyla birlikte yeniden adlandır.
            migrationBuilder.RenameTable(
                name: "Doktorlar",
                newName: "Calisanlar");

            migrationBuilder.RenameIndex(
                name: "IX_Doktorlar_PoliklinikId",
                table: "Calisanlar",
                newName: "IX_Calisanlar_PoliklinikId");

            migrationBuilder.RenameIndex(
                name: "IX_Doktorlar_KullaniciId",
                table: "Calisanlar",
                newName: "IX_Calisanlar_KullaniciId");

            // Sağlık personeli satırlarında doktor unvanı bulunmayabilir.
            migrationBuilder.AlterColumn<string>(
                name: "Unvan",
                table: "Calisanlar",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(80)",
                oldMaxLength: 80);

            // Mevcut kayıtların tamamı doktor olarak işaretlenir.
            migrationBuilder.AddColumn<string>(
                name: "CalisanTuru",
                table: "Calisanlar",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Doktor");

            // Sağlık personeline özgü görev bilgisi.
            migrationBuilder.AddColumn<string>(
                name: "Gorev",
                table: "Calisanlar",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Calisanlar",
                table: "Calisanlar",
                column: "Id");

            // Sağlık personeli için yeni kullanıcı rolü.
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[]
                {
                    "Id",
                    "ConcurrencyStamp",
                    "Name",
                    "NormalizedName"
                },
                values: new object[]
                {
                    "44444444-4444-4444-4444-444444444444",
                    "44444444-4444-4444-4444-444444444444",
                    "SaglikPersoneli",
                    "SAGLIKPERSONELI"
                });

            // İlişkileri yeni tablo adıyla yeniden kur.
            migrationBuilder.AddForeignKey(
                name: "FK_Calisanlar_AspNetUsers_KullaniciId",
                table: "Calisanlar",
                column: "KullaniciId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Calisanlar_Poliklinikler_PoliklinikId",
                table: "Calisanlar",
                column: "PoliklinikId",
                principalTable: "Poliklinikler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eski model sağlık personelini temsil edemez.
            // Böyle kayıtlar varsa geri alma işlemini durdur.
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM "Calisanlar"
                        WHERE "CalisanTuru" <> 'Doktor'
                           OR "Unvan" IS NULL
                           OR "Gorev" IS NOT NULL
                    ) THEN
                        RAISE EXCEPTION
                            'Mevcut çalışan verileri eski doktor modeline kayıpsız dönüştürülemiyor. Geri alma durduruldu.';
                    END IF;

                    IF EXISTS (
                        SELECT 1
                        FROM "AspNetUserRoles"
                        WHERE "RoleId" = '44444444-4444-4444-4444-444444444444'
                    ) OR EXISTS (
                        SELECT 1
                        FROM "AspNetRoleClaims"
                        WHERE "RoleId" = '44444444-4444-4444-4444-444444444444'
                    ) THEN
                        RAISE EXCEPTION
                            'Sağlık personeli rolü kullanımda. Geri alma durduruldu.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Calisanlar_AspNetUsers_KullaniciId",
                table: "Calisanlar");

            migrationBuilder.DropForeignKey(
                name: "FK_Calisanlar_Poliklinikler_PoliklinikId",
                table: "Calisanlar");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Calisanlar",
                table: "Calisanlar");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "44444444-4444-4444-4444-444444444444");

            migrationBuilder.DropColumn(
                name: "CalisanTuru",
                table: "Calisanlar");

            migrationBuilder.DropColumn(
                name: "Gorev",
                table: "Calisanlar");

            migrationBuilder.RenameTable(
                name: "Calisanlar",
                newName: "Doktorlar");

            migrationBuilder.RenameIndex(
                name: "IX_Calisanlar_PoliklinikId",
                table: "Doktorlar",
                newName: "IX_Doktorlar_PoliklinikId");

            migrationBuilder.RenameIndex(
                name: "IX_Calisanlar_KullaniciId",
                table: "Doktorlar",
                newName: "IX_Doktorlar_KullaniciId");

            migrationBuilder.AlterColumn<string>(
                name: "Unvan",
                table: "Doktorlar",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(80)",
                oldMaxLength: 80,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Doktorlar",
                table: "Doktorlar",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Doktorlar_AspNetUsers_KullaniciId",
                table: "Doktorlar",
                column: "KullaniciId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Doktorlar_Poliklinikler_PoliklinikId",
                table: "Doktorlar",
                column: "PoliklinikId",
                principalTable: "Poliklinikler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}