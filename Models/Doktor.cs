using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Models;

public class Doktor : Calisan
{
    public string Unvan { get; set; } = "Uzm. Dr.";

    [MaxLength(150)]
    public string HizmetAdi { get; private set; } = "Doktor muayenesi";

    [Precision(12, 2)]
    public decimal? RandevuUcreti { get; private set; }

    public void HizmetAdiAyarla(string ad)
    {
        var temizAd = ad?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(temizAd))
            throw new ArgumentException("Hizmet adı boş olamaz.");

        if (temizAd.Length > 150)
            throw new ArgumentException(
                "Hizmet adı en fazla 150 karakter olabilir.");

        HizmetAdi = temizAd;
    }

    public void UcretAyarla(decimal ucret)
    {
        UcretKurali.Dogrula(ucret);
        RandevuUcreti = ucret;
    }

    public override string GorunenAdGetir()
    {
        return $"{Unvan} {Ad} {Soyad}".Trim();
    }
}