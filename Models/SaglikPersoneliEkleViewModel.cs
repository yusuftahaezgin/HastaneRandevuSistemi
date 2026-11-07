using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.Models;

public class SaglikPersoneliEkleViewModel
{
    [Required(ErrorMessage = "Görev bilgisi boş olamaz.")]
    [StringLength(80, ErrorMessage = "Görev en fazla 80 karakter olabilir.")]
    public string Gorev { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ad boş olamaz.")]
    [StringLength(80, ErrorMessage = "Ad en fazla 80 karakter olabilir.")]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad boş olamaz.")]
    [StringLength(80, ErrorMessage = "Soyad en fazla 80 karakter olabilir.")]
    public string Soyad { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Poliklinik seçilmelidir.")]
    public int PoliklinikId { get; set; }

    [Required(ErrorMessage = "E-posta adresi boş olamaz.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "İlk şifre boş olamaz.")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Şifre 8 ile 100 karakter arasında olmalıdır.")]
    [DataType(DataType.Password)]
    public string Parola { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrarı boş olamaz.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Parola), ErrorMessage = "Şifreler birbiriyle uyuşmuyor.")]
    public string ParolaTekrar { get; set; } = string.Empty;
}