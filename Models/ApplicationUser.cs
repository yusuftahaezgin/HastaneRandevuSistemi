using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace HastaneRandevuSistemi.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(80)]
    public string Ad { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Soyad { get; set; } = string.Empty;
}