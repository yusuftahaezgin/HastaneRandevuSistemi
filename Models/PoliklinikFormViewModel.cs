using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.Models;

public class PoliklinikFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Poliklinik adı boş olamaz.")]
    [StringLength(
        120,
        ErrorMessage = "Poliklinik adı en fazla 120 karakter olabilir.")]
    public string Ad { get; set; } = string.Empty;

    [StringLength(
        300,
        ErrorMessage = "Açıklama en fazla 300 karakter olabilir.")]
    public string? Aciklama { get; set; }

    public bool AktifMi { get; set; } = true;
}