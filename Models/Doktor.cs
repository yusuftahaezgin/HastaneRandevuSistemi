namespace HastaneRandevuSistemi.Models;

public class Doktor : Calisan
{
    public string Unvan { get; set; } = "Uzm. Dr.";

    public override string GorunenAdGetir()
    {
        return $"{Unvan} {Ad} {Soyad}".Trim();
    }
}