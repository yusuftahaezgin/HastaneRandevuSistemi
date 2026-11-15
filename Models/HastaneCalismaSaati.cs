using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Models;

[Table("HastaneCalismaSaatleri")]
[Index(nameof(SubeId), nameof(Gun), IsUnique = true)]
public class HastaneCalismaSaati
{
    public int Id { get; private set; }

    public int SubeId { get; private set; }

    public Sube Sube { get; private set; } = null!;

    public DayOfWeek Gun { get; private set; }

    public bool AcikMi { get; private set; }

    public int BaslangicDakika { get; private set; }

    public int BitisDakika { get; private set; }

    private HastaneCalismaSaati()
    {
    }

    public HastaneCalismaSaati(DayOfWeek gun)
    {
        if (!Enum.IsDefined(typeof(DayOfWeek), gun))
            throw new ArgumentException("Geçerli bir gün seçin.");

        Gun = gun;

        Guncelle(
            acikMi: gun != DayOfWeek.Sunday,
            baslangicDakika: 0,
            bitisDakika: 1440);
    }

    public void Guncelle(
        bool acikMi,
        int baslangicDakika,
        int bitisDakika)
    {
        if (baslangicDakika < 0
            || baslangicDakika > 1439
            || bitisDakika < 1
            || bitisDakika > 1440
            || bitisDakika <= baslangicDakika)
        {
            throw new ArgumentException(
                "Başlangıç 00:00–23:59, bitiş 00:01–24:00 arasında " +
                "olmalı; bitiş başlangıçtan sonra gelmelidir.");
        }

        AcikMi = acikMi;
        BaslangicDakika = baslangicDakika;
        BitisDakika = bitisDakika;
    }

    public bool Kapsar(TimeOnly baslangic, TimeOnly bitis)
    {
        return AcikMi
            && bitis > baslangic
            && baslangic.ToTimeSpan().TotalMinutes >= BaslangicDakika
            && bitis.ToTimeSpan().TotalMinutes <= BitisDakika;
    }

    public static string SaatMetni(int dakika)
    {
        return $"{dakika / 60:00}:{dakika % 60:00}";
    }

    public static int SaatOku(string? metin, bool bitisSaati)
    {
        metin = metin?.Trim();

        if (bitisSaati && metin == "24:00")
            return 1440;

        if (!TimeOnly.TryParseExact(
                metin,
                "HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var saat))
        {
            throw new ArgumentException(
                "Saatleri 09:00 biçiminde yazın. " +
                "Gün sonu için bitiş saatine 24:00 yazabilirsiniz.");
        }

        return saat.Hour * 60 + saat.Minute;
    }
}