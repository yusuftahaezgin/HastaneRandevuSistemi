using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.Models;

public class DoktorEkleViewModel
{
    [Required(ErrorMessage = "Doktor unvanı seçilmelidir.")]
    [StringLength(80)]
    public string Unvan { get; set; } = "Uzm. Dr.";

    [Required(ErrorMessage = "Doktor adı boş olamaz.")]
    [StringLength(80)]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Doktor soyadı boş olamaz.")]
    [StringLength(80)]
    public string Soyad { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Poliklinik seçilmelidir.")]
    public int PoliklinikId { get; set; }

    [Required(ErrorMessage = "E-posta adresi boş olamaz.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "İlk şifre boş olamaz.")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    public string Parola { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrarı boş olamaz.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Parola), ErrorMessage = "Şifreler birbiriyle uyuşmuyor.")]
    public string ParolaTekrar { get; set; } = string.Empty;
}