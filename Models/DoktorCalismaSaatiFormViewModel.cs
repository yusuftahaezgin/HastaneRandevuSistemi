using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.Models;

public class DoktorCalismaSaatiFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Doktor")]
    [Required(ErrorMessage = "Lütfen bir doktor seçin.")]
    [Range(1, int.MaxValue, ErrorMessage = "Lütfen geçerli bir doktor seçin.")]
    public int? DoktorId { get; set; }

    [Display(Name = "Gün")]
    [Required(ErrorMessage = "Lütfen bir gün seçin.")]
    [EnumDataType(
        typeof(DayOfWeek),
        ErrorMessage = "Lütfen geçerli bir gün seçin.")]
    public DayOfWeek? Gun { get; set; }

    [Display(Name = "Başlangıç Saati")]
    [Required(ErrorMessage = "Başlangıç saatini girin.")]
    [DataType(DataType.Time)]
    [DisplayFormat(
        DataFormatString = "{0:HH:mm}",
        ApplyFormatInEditMode = true)]
    public TimeOnly? BaslangicSaati { get; set; }

    [Display(Name = "Bitiş Saati")]
    [Required(ErrorMessage = "Bitiş saatini girin.")]
    [DataType(DataType.Time)]
    [DisplayFormat(
        DataFormatString = "{0:HH:mm}",
        ApplyFormatInEditMode = true)]
    public TimeOnly? BitisSaati { get; set; }

    [Display(Name = "Randevu Süresi (Dakika)")]
    [Required(ErrorMessage = "Randevu süresini girin.")]
    [Range(
        5,
        120,
        ErrorMessage = "Randevu süresi 5 ile 120 dakika arasında olmalıdır.")]
    public int? RandevuSuresiDakika { get; set; } = 20;

    [Display(Name = "Aktif")]
    public bool AktifMi { get; set; } = true;
}