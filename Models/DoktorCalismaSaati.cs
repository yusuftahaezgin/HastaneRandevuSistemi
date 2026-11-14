namespace HastaneRandevuSistemi.Models;

public class DoktorCalismaSaati
{
    public int Id { get; private set; }

    public int DoktorId { get; private set; }

    public DayOfWeek Gun { get; private set; }

    public TimeOnly BaslangicSaati { get; private set; }

    public TimeOnly BitisSaati { get; private set; }

    public int RandevuSuresiDakika { get; private set; }

    public bool AktifMi { get; private set; } = true;

    public Doktor Doktor { get; private set; } = null!;

    // Entity Framework için.
    private DoktorCalismaSaati()
    {
    }

    public DoktorCalismaSaati(
        int doktorId,
        DayOfWeek gun,
        TimeOnly baslangicSaati,
        TimeOnly bitisSaati,
        int randevuSuresiDakika,
        bool aktifMi = true)
    {
        if (doktorId <= 0)
        {
            throw new ArgumentException(
                "Lütfen geçerli bir doktor seçin.");
        }

        BilgileriGuncelle(
            gun,
            baslangicSaati,
            bitisSaati,
            randevuSuresiDakika);

        DoktorId = doktorId;
        AktifMi = aktifMi;
    }

    public void BilgileriGuncelle(
        DayOfWeek gun,
        TimeOnly baslangicSaati,
        TimeOnly bitisSaati,
        int randevuSuresiDakika)
    {
        if (!Enum.IsDefined(typeof(DayOfWeek), gun))
        {
            throw new ArgumentException(
                "Lütfen geçerli bir gün seçin.");
        }

        if (baslangicSaati.Ticks % TimeSpan.TicksPerMinute != 0
            || bitisSaati.Ticks % TimeSpan.TicksPerMinute != 0)
        {
            throw new ArgumentException(
                "Çalışma saatlerini saat ve dakika olarak girin.");
        }

        if (bitisSaati <= baslangicSaati)
        {
            throw new ArgumentException(
                "Bitiş saati başlangıç saatinden sonra olmalıdır. " +
                "Çalışma aralığı aynı gün içinde olmalıdır.");
        }

        if (randevuSuresiDakika < 5 || randevuSuresiDakika > 120)
        {
            throw new ArgumentException(
                "Randevu süresi 5 ile 120 dakika arasında olmalıdır.");
        }

        var calismaSuresi = bitisSaati.ToTimeSpan()
                           - baslangicSaati.ToTimeSpan();

        if (calismaSuresi.TotalMinutes < randevuSuresiDakika)
        {
            throw new ArgumentException(
                "Çalışma aralığı en az bir randevu süresi kadar olmalıdır.");
        }

        Gun = gun;
        BaslangicSaati = baslangicSaati;
        BitisSaati = bitisSaati;
        RandevuSuresiDakika = randevuSuresiDakika;
    }

    public void AktiflikAyarla(bool aktifMi)
    {
        AktifMi = aktifMi;
    }

    public void DurumDegistir()
    {
        AktifMi = !AktifMi;
    }
}