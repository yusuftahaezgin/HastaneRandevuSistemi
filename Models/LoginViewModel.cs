using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı veya e-posta gereklidir.")]
    public string KullaniciAdiVeyaEposta { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola gereklidir.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool BeniHatirla { get; set; }

    public string? ReturnUrl { get; set; }
}