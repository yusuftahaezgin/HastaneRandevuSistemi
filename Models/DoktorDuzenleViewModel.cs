using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.Models;

public class DoktorDuzenleViewModel
{
    public int Id { get; set; }

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

    public string? HesapEpostasi { get; set; }
}