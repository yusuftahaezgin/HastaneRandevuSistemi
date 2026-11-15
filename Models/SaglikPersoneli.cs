namespace HastaneRandevuSistemi.Models;

public class SaglikPersoneli : Calisan
{
    public string Gorev { get; set; } = string.Empty;

    public ICollection<PersonelHizmeti> Hizmetler { get; set; }
        = new List<PersonelHizmeti>();

    public ICollection<PersonelCalismaSaati> CalismaSaatleri { get; set; }
        = new List<PersonelCalismaSaati>();

    public override string GorunenAdGetir()
    {
        return $"{Gorev} {Ad} {Soyad}".Trim();
    }
}