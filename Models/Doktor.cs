namespace HastaneRandevuSistemi.Models;

public class Doktor
{
    public int Id { get; set; }

    public string Unvan { get; set; } = "Uzm. Dr.";

    public string Ad { get; set; } = string.Empty;

    public string Soyad { get; set; } = string.Empty;

    public int PoliklinikId { get; set; }

    public bool AktifMi { get; set; } = true;

    // Doktorun bağlı olduğu poliklinik
    public Poliklinik Poliklinik { get; set; } = null!;
}