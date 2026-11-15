using System.Security.Claims;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers;

[Authorize(Roles = "Doktor,SaglikPersoneli")]
public class CalisanController : Controller
{
    private readonly AppDbContext db;

    public CalisanController(AppDbContext context)
    {
        db = context;
    }

    [HttpGet]
    public async Task<IActionResult> Randevular()
    {
        var kullaniciId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(kullaniciId))
            return Challenge();

        var calisan = await db.Calisanlar
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.KullaniciId == kullaniciId);

        if (calisan is null)
        {
            ViewBag.Uyari =
                "Hesabınıza bağlı çalışan kaydı bulunamadı. " +
                "Yöneticiyle iletişime geçin.";

            return View(new List<Randevu>());
        }

        var rolUygun =
            (calisan is Doktor && User.IsInRole("Doktor"))
            || (calisan is SaglikPersoneli
                && User.IsInRole("SaglikPersoneli"));

        if (!rolUygun)
        {
            ViewBag.Uyari =
                "Hesabınızın rolü ile çalışan kaydı eşleşmiyor. " +
                "Yöneticiyle iletişime geçin.";

            return View(new List<Randevu>());
        }

        ViewBag.CalisanAdi = calisan.GorunenAdGetir();

        var randevular = await db.Set<Randevu>()
            .AsNoTracking()
            .Include(x => x.Hasta)
            .Where(x => x.CalisanId == calisan.Id)
            .OrderByDescending(x => x.Tarih)
            .ThenBy(x => x.Baslangic)
            .ToListAsync();

        return View(randevular);
    }
}