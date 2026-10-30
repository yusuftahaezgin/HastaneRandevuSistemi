namespace HastaneRandevuSistemi.Models;

public class RandevuSecimViewModel
{
    public List<Poliklinik> Poliklinikler { get; set; } = new();

    public List<Doktor> Doktorlar { get; set; } = new();

    public int? SeciliPoliklinikId { get; set; }

    public int? SeciliDoktorId { get; set; }

    public Doktor? SeciliDoktor { get; set; }
}