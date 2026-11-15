using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Models;

public class Doktor : Calisan
{
    public string Unvan { get; set; } = "Uzm. Dr.";

    [Precision(12, 2)]
    public decimal? RandevuUcreti { get; private set; }

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