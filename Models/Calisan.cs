namespace HastaneRandevuSistemi.Models;

public abstract class Calisan
{
    public int Id { get; set; }

    public string Ad { get; set; } = string.Empty;

    public string Soyad { get; set; } = string.Empty;

    public int PoliklinikId { get; set; }

    public bool AktifMi { get; set; } = true;

    public Poliklinik Poliklinik { get; set; } = null!;

    public string? KullaniciId { get; set; }

    public ApplicationUser? Kullanici { get; set; }

    public abstract string GorunenAdGetir();
}