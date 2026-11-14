using System.Globalization;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevuSistemi.Controllers;

[Authorize(Roles = "Hasta")]
public class RandevuController : Controller
{
    private readonly IRandevuSecimService _randevuSecimService;

    public RandevuController(
        IRandevuSecimService randevuSecimService)
    {
        _randevuSecimService = randevuSecimService;
    }

    [HttpGet]
    public async Task<IActionResult> Olustur(
        int? kurumId,
        int? subeId,
        int? poliklinikId,
        int? doktorId,
        string? tarih)
    {
        if (!ModelState.IsValid)
        {
            ModelState.Clear();

            ModelState.AddModelError(
                string.Empty,
                "Seçim bilgileri geçersiz. Lütfen yeniden seçim yapın.");

            return View(
                await _randevuSecimService.HazirlaAsync());
        }

        try
        {
            DateOnly? seciliTarih = null;

            // İlk açılışta tarih parametresi bulunmaz.
            // Tarih formu gönderildiyse boş veya hatalı değerleri reddederiz.
            if (Request.Query.ContainsKey("tarih"))
            {
                if (!DateOnly.TryParseExact(
                        tarih,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var parsedTarih))
                {
                    throw new ArgumentException(
                        "Lütfen geçerli bir tarih seçin.");
                }

                seciliTarih = parsedTarih;
            }

            var model = await _randevuSecimService.HazirlaAsync(
                kurumId,
                subeId,
                poliklinikId,
                doktorId,
                seciliTarih);

            return View(model);
        }
        catch (ArgumentException ex)
        {
            ModelState.Clear();
            ModelState.AddModelError(string.Empty, ex.Message);

            // Tarih hatalıysa geçerli doktor ve hastane seçimlerini koruruz.
            var model = await SecimleriKoruAsync(
                kurumId,
                subeId,
                poliklinikId,
                doktorId);

            return View(model);
        }
    }

    private async Task<RandevuSecimViewModel> SecimleriKoruAsync(
        int? kurumId,
        int? subeId,
        int? poliklinikId,
        int? doktorId)
    {
        try
        {
            return await _randevuSecimService.HazirlaAsync(
                kurumId,
                subeId,
                poliklinikId,
                doktorId);
        }
        catch (ArgumentException)
        {
            // Doktor veya üst seçimler de geçersizse
            // ilk seçim ekranına dönülür.
            return await _randevuSecimService.HazirlaAsync();
        }
    }
}