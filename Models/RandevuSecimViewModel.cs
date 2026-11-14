namespace HastaneRandevuSistemi.Models;

public class RandevuSecimViewModel
{
    public List<Kurum> Kurumlar { get; set; } = new();

    public List<Sube> Subeler { get; set; } = new();

    public List<Poliklinik> Poliklinikler { get; set; } = new();

    public List<Doktor> Doktorlar { get; set; } = new();

    public int? SeciliKurumId { get; set; }

    public int? SeciliSubeId { get; set; }

    public int? SeciliPoliklinikId { get; set; }

    public int? SeciliDoktorId { get; set; }

    public Doktor? SeciliDoktor { get; set; }
}