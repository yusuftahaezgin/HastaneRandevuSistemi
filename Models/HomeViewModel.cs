using System.Collections.Generic;

namespace HastaneRandevuSistemi.Models;

public class HomeViewModel
{
    public List<Poliklinik> Poliklinikler { get; set; }
        = new List<Poliklinik>();

    public List<Doktor> Doktorlar { get; set; }
        = new List<Doktor>();
}