namespace HastaneRandevuSistemi.Models;

public class Sube
{
    public int Id { get; private set; }

    public int KurumId { get; private set; }

    public string Ad { get; private set; } = string.Empty;

    public string Adres { get; private set; } = string.Empty;

    public string Telefon { get; private set; } = string.Empty;

    public bool AktifMi { get; private set; } = true;

    public Kurum Kurum { get; private set; } = null!;

    private Sube()
    {
    }

    public Sube(
        int kurumId,
        string ad,
        string adres,
        string? telefon,
        bool aktifMi = true)
    {
        if (kurumId <= 0)
        {
            throw new ArgumentException("Geçerli bir kurum seçilmelidir.");
        }

        BilgileriGuncelle(ad, adres, telefon);

        KurumId = kurumId;
        AktiflikAyarla(aktifMi);
    }

    public void BilgileriGuncelle(
        string ad,
        string adres,
        string? telefon)
    {
        var temizAd = ad?.Trim() ?? string.Empty;
        var temizAdres = adres?.Trim() ?? string.Empty;
        var temizTelefon = telefon?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(temizAd))
        {
            throw new ArgumentException("Şube adı boş olamaz.");
        }

        if (temizAd.Length > 150)
        {
            throw new ArgumentException(
                "Şube adı en fazla 150 karakter olabilir.");
        }

        if (string.IsNullOrWhiteSpace(temizAdres))
        {
            throw new ArgumentException("Şube adresi boş olamaz.");
        }

        if (temizAdres.Length > 400)
        {
            throw new ArgumentException(
                "Adres en fazla 400 karakter olabilir.");
        }

        if (temizTelefon.Length > 30)
        {
            throw new ArgumentException(
                "Telefon en fazla 30 karakter olabilir.");
        }

        Ad = temizAd;
        Adres = temizAdres;
        Telefon = temizTelefon;
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