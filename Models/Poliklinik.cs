namespace HastaneRandevuSistemi.Models;

public class Poliklinik
{
    public int Id { get; set; }

    public string Ad { get; set; } = string.Empty;

    public string Aciklama { get; set; } = string.Empty;

    public bool AktifMi { get; set; } = true;

    public ICollection<Doktor> Doktorlar { get; set; }
    = new List<Doktor>();
}