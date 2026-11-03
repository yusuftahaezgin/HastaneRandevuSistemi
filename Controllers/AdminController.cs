using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HastaneRandevuSistemi.Controllers;

[Authorize(Roles = "Yonetici")]
public class AdminController : Controller
{
    private readonly IPoliklinikService _poliklinikService;
    private readonly IDoktorService _doktorService;

    public AdminController(
        IPoliklinikService poliklinikService,
        IDoktorService doktorService)
    {
        _poliklinikService = poliklinikService;
        _doktorService = doktorService;
    }

    public IActionResult Index()
    {
        return View();
    }

    // ---------------- POLİKLİNİK İŞLEMLERİ ----------------

    [HttpGet]
    public async Task<IActionResult> Poliklinikler()
    {
        var poliklinikler = await _poliklinikService.ListeleAsync();

        return View(poliklinikler);
    }

    [HttpGet]
    public IActionResult PoliklinikEkle()
    {
        return View(new PoliklinikFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PoliklinikEkle(
        PoliklinikFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _poliklinikService.EkleAsync(
                model.Ad,
                model.Aciklama,
                model.AktifMi);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        TempData["Basari"] = "Poliklinik başarıyla eklendi.";

        return RedirectToAction(nameof(Poliklinikler));
    }

    [HttpGet]
    public async Task<IActionResult> PoliklinikDuzenle(int id)
    {
        var poliklinik = await _poliklinikService.GetirAsync(id);

        if (poliklinik is null)
        {
            return NotFound();
        }

        var model = new PoliklinikFormViewModel
        {
            Id = poliklinik.Id,
            Ad = poliklinik.Ad,
            Aciklama = poliklinik.Aciklama,
            AktifMi = poliklinik.AktifMi
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PoliklinikDuzenle(
        PoliklinikFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            bool bulundu = await _poliklinikService.GuncelleAsync(
                model.Id,
                model.Ad,
                model.Aciklama,
                model.AktifMi);

            if (!bulundu)
            {
                return NotFound();
            }
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        TempData["Basari"] = "Poliklinik başarıyla güncellendi.";

        return RedirectToAction(nameof(Poliklinikler));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DurumDegistir(int id)
    {
        bool bulundu = await _poliklinikService.DurumDegistirAsync(id);

        if (!bulundu)
        {
            return NotFound();
        }

        TempData["Basari"] = "Poliklinik durumu güncellendi.";

        return RedirectToAction(nameof(Poliklinikler));
    }

    // ---------------- DOKTOR İŞLEMLERİ ----------------

    [HttpGet]
    public async Task<IActionResult> Doktorlar()
    {
        var doktorlar = await _doktorService.ListeleAsync();

        return View(doktorlar);
    }

    [HttpGet]
    public async Task<IActionResult> DoktorEkle()
    {
        await PoliklinikListesiniHazirla();

        return View(new DoktorEkleViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoktorEkle(
        DoktorEkleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PoliklinikListesiniHazirla(model.PoliklinikId);
            return View(model);
        }

        try
        {
            await _doktorService.EkleAsync(model);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            await PoliklinikListesiniHazirla(model.PoliklinikId);
            return View(model);
        }

        TempData["Basari"] =
            "Doktor ve giriş hesabı başarıyla oluşturuldu.";

        return RedirectToAction(nameof(Doktorlar));
    }

    [HttpGet]
    public async Task<IActionResult> DoktorDuzenle(int id)
    {
        var doktor = await _doktorService.GetirAsync(id);

        if (doktor is null)
        {
            return NotFound();
        }

        var model = new DoktorDuzenleViewModel
        {
            Id = doktor.Id,
            Unvan = doktor.Unvan,
            Ad = doktor.Ad,
            Soyad = doktor.Soyad,
            PoliklinikId = doktor.PoliklinikId,
            HesapEpostasi = doktor.Kullanici?.Email
        };

        await PoliklinikListesiniHazirla(
            doktor.PoliklinikId,
            pasifleriDahilEt: true);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoktorDuzenle(
        DoktorDuzenleViewModel model)
    {
        var doktor = await _doktorService.GetirAsync(model.Id);

        if (doktor is null)
        {
            return NotFound();
        }

        // Form tekrar gösterilirse hesap e-postası veritabanından alınır.
        model.HesapEpostasi = doktor.Kullanici?.Email;

        if (!ModelState.IsValid)
        {
            await PoliklinikListesiniHazirla(
                model.PoliklinikId,
                pasifleriDahilEt: true);

            return View(model);
        }

        try
        {
            bool bulundu = await _doktorService.GuncelleAsync(model);

            if (!bulundu)
            {
                return NotFound();
            }
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            await PoliklinikListesiniHazirla(
                model.PoliklinikId,
                pasifleriDahilEt: true);

            return View(model);
        }

        TempData["Basari"] = "Doktor bilgileri güncellendi.";

        return RedirectToAction(nameof(Doktorlar));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoktorDurumDegistir(int id)
    {
        bool bulundu = await _doktorService.DurumDegistirAsync(id);

        if (!bulundu)
        {
            return NotFound();
        }

        TempData["Basari"] = "Doktor durumu güncellendi.";

        return RedirectToAction(nameof(Doktorlar));
    }

    // Doktor formlarının poliklinik seçeneklerini hazırlar.
    private async Task PoliklinikListesiniHazirla(
        int? seciliId = null,
        bool pasifleriDahilEt = false)
    {
        var poliklinikler = await _poliklinikService.ListeleAsync();

        IEnumerable<Poliklinik> secenekler = poliklinikler;

        if (!pasifleriDahilEt)
        {
            secenekler = secenekler.Where(p => p.AktifMi);
        }

        ViewBag.Poliklinikler = secenekler
            .OrderBy(p => p.Ad)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Ad + (p.AktifMi ? "" : " (Pasif)"),
                Selected = p.Id == seciliId
            })
            .ToList();
    }
}