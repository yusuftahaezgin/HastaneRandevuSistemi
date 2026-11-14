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
        int? doktorId)
    {
        // Örneğin adres çubuğundan kurumId=abc gönderilmesi.
        if (!ModelState.IsValid)
        {
            ModelState.Clear();

            ModelState.AddModelError(
                string.Empty,
                "Seçim bilgileri geçersiz. Lütfen yeniden seçim yapın.");

            var bosModel = await _randevuSecimService.HazirlaAsync();

            return View(bosModel);
        }

        try
        {
            var model = await _randevuSecimService.HazirlaAsync(
                kurumId: kurumId,
                subeId: subeId,
                poliklinikId: poliklinikId,
                doktorId: doktorId);

            return View(model);
        }
        catch (ArgumentException ex)
        {
            ModelState.Clear();
            ModelState.AddModelError(string.Empty, ex.Message);

            var bosModel = await _randevuSecimService.HazirlaAsync();

            return View(bosModel);
        }
    }
}