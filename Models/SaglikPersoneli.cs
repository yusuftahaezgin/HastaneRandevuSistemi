namespace HastaneRandevuSistemi.Models;

public class SaglikPersoneli : Calisan
{
    public string Gorev { get; set; } = string.Empty;

    public override string GorunenAdGetir()
    {
        return $"{Gorev} {Ad} {Soyad}".Trim();
    }
}