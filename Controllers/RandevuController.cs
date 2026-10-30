using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers;

[Authorize(Roles = "Hasta")]
public class RandevuController : Controller
{
    private readonly AppDbContext _context;

    public RandevuController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Olustur(
        int? poliklinikId,
        int? doktorId)
    {
        var model = new RandevuSecimViewModel
        {
            Poliklinikler = await _context.Poliklinikler
                .Where(p => p.AktifMi)
                .OrderBy(p => p.Id)
                .ToListAsync()
        };

        if (!poliklinikId.HasValue)
        {
            return View(model);
        }

        var poliklinik = model.Poliklinikler
            .FirstOrDefault(p => p.Id == poliklinikId.Value);

        if (poliklinik is null)
        {
            return NotFound();
        }

        model.SeciliPoliklinikId = poliklinik.Id;

        model.Doktorlar = await _context.Doktorlar
            .Include(d => d.Poliklinik)
            .Where(d =>
                d.AktifMi
                && d.Poliklinik.AktifMi
                && d.PoliklinikId == poliklinik.Id)
            .OrderBy(d => d.Ad)
            .ThenBy(d => d.Soyad)
            .ToListAsync();

        if (doktorId.HasValue)
        {
            model.SeciliDoktor = model.Doktorlar
                .FirstOrDefault(d => d.Id == doktorId.Value);

            model.SeciliDoktorId = model.SeciliDoktor?.Id;
        }

        return View(model);
    }
}