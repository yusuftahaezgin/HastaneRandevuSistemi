namespace HastaneRandevuSistemi.Models;

public class Kurum
{
    public int Id { get; private set; }

    public string Ad { get; private set; } = string.Empty;

    public string Aciklama { get; private set; } = string.Empty;

    public bool AktifMi { get; private set; } = true;

    public ICollection<Sube> Subeler { get; private set; }
        = new List<Sube>();

    private Kurum()
    {
    }

    public Kurum(
        string ad,
        string? aciklama,
        bool aktifMi = true)
    {
        BilgileriGuncelle(ad, aciklama);
        AktiflikAyarla(aktifMi);
    }

    public void BilgileriGuncelle(
        string ad,
        string? aciklama)
    {
        var temizAd = ad?.Trim() ?? string.Empty;
        var temizAciklama = aciklama?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(temizAd))
        {
            throw new ArgumentException("Kurum adı boş olamaz.");
        }

        if (temizAd.Length > 150)
        {
            throw new ArgumentException(
                "Kurum adı en fazla 150 karakter olabilir.");
        }

        if (temizAciklama.Length > 300)
        {
            throw new ArgumentException(
                "Açıklama en fazla 300 karakter olabilir.");
        }

        Ad = temizAd;
        Aciklama = temizAciklama;
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