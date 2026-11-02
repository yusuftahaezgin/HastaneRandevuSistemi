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
        int? poliklinikId,
        int? doktorId)
    {
        var model = await _randevuSecimService.HazirlaAsync(
            poliklinikId,
            doktorId);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }
}