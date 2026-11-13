using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.Models;

public class KurumFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Kurum Adı")]
    [Required(ErrorMessage = "Kurum adı zorunludur.")]
    [StringLength(
        150,
        ErrorMessage = "Kurum adı en fazla 150 karakter olabilir.")]
    public string Ad { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    [StringLength(
        300,
        ErrorMessage = "Açıklama en fazla 300 karakter olabilir.")]
    public string? Aciklama { get; set; }

    [Display(Name = "Aktif")]
    public bool AktifMi { get; set; } = true;
}