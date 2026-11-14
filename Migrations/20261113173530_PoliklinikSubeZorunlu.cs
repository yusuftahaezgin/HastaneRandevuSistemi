using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HastaneRandevuSistemi.Migrations
{
    public partial class PoliklinikSubeZorunlu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    -- Başlangıç polikliniklerinde boş bağlantı varsa
                    -- bunları tamamlamadan NOT NULL uygulanamaz.
                    IF EXISTS (
                        SELECT 1
                        FROM public."Poliklinikler"
                        WHERE "Id" BETWEEN 1 AND 12
                          AND "SubeId" IS NULL
                    ) THEN

                        -- Proje boş bir veritabanına kuruluyorsa
                        -- örnek poliklinikler için kurum ve hastane oluştur.
                        IF NOT EXISTS (
                            SELECT 1 FROM public."Kurumlar"
                        ) AND NOT EXISTS (
                            SELECT 1 FROM public."Subeler"
                        ) THEN

                            INSERT INTO public."Kurumlar"
                                ("Id", "Ad", "Aciklama", "AktifMi")
                            VALUES (
                                1,
                                'Eğitim ve Araştırma Hastaneleri',
                                'Türkiye''de sağlık hizmeti sunan eğitim ve araştırma hastaneleri.',
                                TRUE
                            );

                            INSERT INTO public."Subeler"
                                (
                                    "Id",
                                    "KurumId",
                                    "Ad",
                                    "Adres",
                                    "Telefon",
                                    "AktifMi"
                                )
                            VALUES (
                                2,
                                1,
                                'Sakarya Eğitim ve Araştırma Hastanesi',
                                'Şirinevler, Adnan Menderes Cd Sağlık Sk No:195, 54100 Adapazarı/Sakarya',
                                '444 5 400',
                                TRUE
                            );

                            -- Kimlikleri elle verdiğimiz için
                            -- otomatik numara üretimini de ilerlet.
                            PERFORM setval(
                                pg_get_serial_sequence(
                                    'public."Kurumlar"',
                                    'Id'
                                ),
                                1,
                                TRUE
                            );

                            PERFORM setval(
                                pg_get_serial_sequence(
                                    'public."Subeler"',
                                    'Id'
                                ),
                                2,
                                TRUE
                            );
                        END IF;

                        -- Yanlış hastaneye otomatik atama yapılmasın.
                        IF NOT EXISTS (
                            SELECT 1
                            FROM public."Subeler" s
                            INNER JOIN public."Kurumlar" k
                                ON k."Id" = s."KurumId"
                            WHERE s."Id" = 2
                              AND s."Ad" =
                                  'Sakarya Eğitim ve Araştırma Hastanesi'
                              AND k."Ad" =
                                  'Eğitim ve Araştırma Hastaneleri'
                        ) THEN
                            RAISE EXCEPTION
                                'Başlangıç poliklinikleri için beklenen hastane bulunamadı. Hastane atamalarını kontrol edin.';
                        END IF;

                        -- Yalnızca bağlantısı boş başlangıç kayıtları.
                        -- Önceden atanmış hastaneler değiştirilmez.
                        UPDATE public."Poliklinikler"
                        SET "SubeId" = 2
                        WHERE "Id" BETWEEN 1 AND 12
                          AND "SubeId" IS NULL;
                    END IF;

                    -- Başlangıç kayıtları dışındaki eksik atamaları
                    -- tahmin ederek doldurmak yerine işlemi durdur.
                    IF EXISTS (
                        SELECT 1
                        FROM public."Poliklinikler"
                        WHERE "SubeId" IS NULL
                    ) THEN
                        RAISE EXCEPTION
                            'Hastane atanmamış poliklinikler var. Yönetim panelinden hastanelerini seçip migration işlemini tekrar çalıştırın.';
                    END IF;
                END $$;
                """
            );

            migrationBuilder.AlterColumn<int>(
                name: "SubeId",
                table: "Poliklinikler",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alındığında mevcut hastane atamaları korunur.
            // Sadece sütunun boş değer kabul etmesine izin verilir.
            migrationBuilder.AlterColumn<int>(
                name: "SubeId",
                table: "Poliklinikler",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}