using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.Models;

public class SubeFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Kurum")]
    [Range(1, int.MaxValue, ErrorMessage = "Bir kurum seçmelisiniz.")]
    public int KurumId { get; set; }

    [Display(Name = "Şube / Hastane Adı")]
    [Required(ErrorMessage = "Şube adı zorunludur.")]
    [StringLength(
        150,
        ErrorMessage = "Şube adı en fazla 150 karakter olabilir.")]
    public string Ad { get; set; } = string.Empty;

    [Display(Name = "Adres")]
    [Required(ErrorMessage = "Adres zorunludur.")]
    [StringLength(
        400,
        ErrorMessage = "Adres en fazla 400 karakter olabilir.")]
    public string Adres { get; set; } = string.Empty;

    [Display(Name = "Telefon")]
    [StringLength(
        30,
        ErrorMessage = "Telefon en fazla 30 karakter olabilir.")]
    public string? Telefon { get; set; }

    [Display(Name = "Aktif")]
    public bool AktifMi { get; set; } = true;
}