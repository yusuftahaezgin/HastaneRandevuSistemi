using System.Globalization;
using System.Security.Claims;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services;
using HastaneRandevuSistemi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevuSistemi.Controllers;

[Authorize(Roles = "Hasta")]
public class RandevuController : Controller
{
    private readonly IRandevuSecimService _secimService;
    private readonly RandevuKayitService _kayitService;

    public RandevuController(
        IRandevuSecimService randevuSecimService,
        AppDbContext db)
    {
        _secimService = randevuSecimService;
        _kayitService = new RandevuKayitService(db);
    }

    private string HastaId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException(
            "Hasta kimliği bulunamadı. Lütfen tekrar giriş yapın.");

    [HttpGet]
    public async Task<IActionResult> Olustur(
        int? kurumId,
        int? subeId,
        int? poliklinikId,
        int? doktorId,
        string? tarih)
    {
        RandevuSecimViewModel model;

        try
        {
            FormKontrol();

            DateOnly? seciliTarih =
                Request.Query.ContainsKey("tarih")
                    ? TarihOku(tarih)
                    : null;

            model = await _secimService.HazirlaAsync(
                kurumId,
                subeId,
                poliklinikId,
                doktorId,
                seciliTarih);
        }
        catch (ArgumentException ex)
        {
            ModelState.Clear();
            ModelState.AddModelError(string.Empty, ex.Message);

            try
            {
                model = await _secimService.HazirlaAsync(
                    kurumId,
                    subeId,
                    poliklinikId,
                    doktorId);
            }
            catch (ArgumentException)
            {
                model = await _secimService.HazirlaAsync();
            }
        }

        await _kayitService.DoktorSaatleriniFiltreleAsync(
            model,
            HastaId);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Hizmet(
        int? hizmetId,
        string? tarih)
    {
        try
        {
            FormKontrol();

            var seciliTarih =
                Request.Query.ContainsKey("tarih")
                    ? TarihOku(tarih)
                    : DateOnly.FromDateTime(RandevuKayitService.Simdi);

            var model = await _kayitService.PersonelSecimAsync(
                hizmetId,
                seciliTarih,
                HastaId);

            return View(model);
        }
        catch (ArgumentException ex)
        {
            ModelState.Clear();
            ModelState.AddModelError(string.Empty, ex.Message);

            var model = await _kayitService.PersonelSecimAsync(
                null,
                DateOnly.FromDateTime(RandevuKayitService.Simdi),
                HastaId);

            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Liste()
    {
        var randevular = await _kayitService.ListeAsync(HastaId);
        return View(randevular);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoktorKaydet(
        int doktorId,
        int poliklinikId,
        string tarih,
        string saat,
        string ucret,
        int sure)
    {
        try
        {
            FormKontrol();

            await _kayitService.DoktorKaydetAsync(
                HastaId,
                doktorId,
                TarihOku(tarih),
                SaatOku(saat),
                RandevuKayitService.UcretOku(ucret),
                sure);

            TempData["Basari"] =
                "Randevu talebiniz kaydedildi. Yönetici onayı bekleniyor.";

            return RedirectToAction(nameof(Liste));
        }
        catch (ArgumentException ex)
        {
            TempData["Hata"] = ex.Message;

            return RedirectToAction(
                nameof(Olustur),
                new
                {
                    poliklinikId,
                    doktorId,
                    tarih
                });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HizmetKaydet(
        int hizmetId,
        string tarih,
        string saat,
        string ucret,
        int sure)
    {
        try
        {
            FormKontrol();

            await _kayitService.PersonelKaydetAsync(
                HastaId,
                hizmetId,
                TarihOku(tarih),
                SaatOku(saat),
                RandevuKayitService.UcretOku(ucret),
                sure);

            TempData["Basari"] =
                "Hizmet randevunuz kaydedildi. Yönetici onayı bekleniyor.";

            return RedirectToAction(nameof(Liste));
        }
        catch (ArgumentException ex)
        {
            TempData["Hata"] = ex.Message;

            return RedirectToAction(
                nameof(Hizmet),
                new
                {
                    hizmetId,
                    tarih
                });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Iptal(int id)
    {
        try
        {
            FormKontrol();

            await _kayitService.IptalAsync(id, HastaId);

            TempData["Basari"] = "Randevunuz iptal edildi.";
        }
        catch (ArgumentException ex)
        {
            TempData["Hata"] = ex.Message;
        }

        return RedirectToAction(nameof(Liste));
    }

    private void FormKontrol()
    {
        if (!ModelState.IsValid)
        {
            throw new ArgumentException(
                "Seçim bilgileri geçersiz. Lütfen bilgileri kontrol edin.");
        }
    }

    private static DateOnly TarihOku(string? tarih)
    {
        if (!DateOnly.TryParseExact(
                tarih,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var sonuc))
        {
            throw new ArgumentException("Geçerli bir tarih seçin.");
        }

        return sonuc;
    }

    private static TimeOnly SaatOku(string? saat)
    {
        if (!TimeOnly.TryParseExact(
                saat,
                "HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var sonuc))
        {
            throw new ArgumentException("Geçerli bir saat seçin.");
        }

        return sonuc;
    }
}