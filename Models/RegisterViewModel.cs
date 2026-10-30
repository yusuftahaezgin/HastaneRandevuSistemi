using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad alanı gereklidir.")]
    [MaxLength(80)]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı gereklidir.")]
    [MaxLength(80)]
    public string Soyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresi gereklidir.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola gereklidir.")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Parola en az 8 karakter olmalıdır.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola tekrarı gereklidir.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Parolalar eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}