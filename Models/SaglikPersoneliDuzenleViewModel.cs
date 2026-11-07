using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.Models;

public class SaglikPersoneliDuzenleViewModel
{
    public int Id { get; set; }

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

    public string? HesapEpostasi { get; set; }
}