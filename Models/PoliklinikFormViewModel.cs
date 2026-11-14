using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.Models;

public class PoliklinikFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Şube / Hastane")]
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Lütfen bir hastane seçin.")]
    public int SubeId { get; set; }

    [Display(Name = "Poliklinik Adı")]
    [Required(ErrorMessage = "Poliklinik adı boş olamaz.")]
    [StringLength(
        120,
        ErrorMessage = "Poliklinik adı en fazla 120 karakter olabilir.")]
    public string Ad { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    [StringLength(
        300,
        ErrorMessage = "Açıklama en fazla 300 karakter olabilir.")]
    public string? Aciklama { get; set; }

    [Display(Name = "Aktif")]
    public bool AktifMi { get; set; } = true;
}