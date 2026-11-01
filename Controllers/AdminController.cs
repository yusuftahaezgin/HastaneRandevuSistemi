using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers;

[Authorize(Roles = "Yonetici")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(
        AppDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    // ---------------- POLİKLİNİK İŞLEMLERİ ----------------

    public async Task<IActionResult> Poliklinikler()
    {
        var poliklinikler = await _context.Poliklinikler
            .OrderBy(p => p.Ad)
            .ToListAsync();

        return View(poliklinikler);
    }

    [HttpGet]
    public IActionResult PoliklinikEkle()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PoliklinikEkle(
        string ad, string aciklama)
    {
        ad = ad?.Trim() ?? "";
        aciklama = aciklama?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(ad))
        {
            ModelState.AddModelError("ad", "Poliklinik adı boş olamaz.");
            return View();
        }

        if (ad.Length > 120 || aciklama.Length > 300)
        {
            ModelState.AddModelError("", "Girilen bilgiler çok uzun.");
            return View();
        }

        bool mevcut = await _context.Poliklinikler
            .AnyAsync(p => p.Ad.ToLower() == ad.ToLower());

        if (mevcut)
        {
            ModelState.AddModelError("ad", "Bu poliklinik zaten mevcut.");
            return View();
        }

        var poliklinik = new Poliklinik
        {
            Ad = ad,
            Aciklama = aciklama,
            AktifMi = true
        };

        _context.Poliklinikler.Add(poliklinik);
        await _context.SaveChangesAsync();

        TempData["Basari"] = "Poliklinik başarıyla eklendi.";

        return RedirectToAction(nameof(Poliklinikler));
    }

    [HttpGet]
    public async Task<IActionResult> PoliklinikDuzenle(int id)
    {
        var poliklinik = await _context.Poliklinikler
            .FindAsync(id);

        if (poliklinik == null)
            return NotFound();

        return View(poliklinik);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PoliklinikDuzenle(
        int id, string ad, string aciklama)
    {
        ad = ad?.Trim() ?? "";
        aciklama = aciklama?.Trim() ?? "";

        var poliklinik = await _context.Poliklinikler
            .FindAsync(id);

        if (poliklinik == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(ad))
        {
            ModelState.AddModelError("ad", "Poliklinik adı boş olamaz.");
            return View(poliklinik);
        }

        if (ad.Length > 120 || aciklama.Length > 300)
        {
            ModelState.AddModelError("", "Girilen bilgiler çok uzun.");
            return View(poliklinik);
        }

        bool mevcut = await _context.Poliklinikler
            .AnyAsync(p => p.Id != id &&
                           p.Ad.ToLower() == ad.ToLower());

        if (mevcut)
        {
            ModelState.AddModelError("ad", "Bu poliklinik zaten mevcut.");
            return View(poliklinik);
        }

        poliklinik.Ad = ad;
        poliklinik.Aciklama = aciklama;

        await _context.SaveChangesAsync();

        TempData["Basari"] = "Poliklinik başarıyla güncellendi.";

        return RedirectToAction(nameof(Poliklinikler));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DurumDegistir(int id)
    {
        var poliklinik = await _context.Poliklinikler
            .FindAsync(id);

        if (poliklinik == null)
            return NotFound();

        poliklinik.AktifMi = !poliklinik.AktifMi;

        await _context.SaveChangesAsync();

        TempData["Basari"] = "Poliklinik durumu güncellendi.";

        return RedirectToAction(nameof(Poliklinikler));
    }

    // ---------------- DOKTOR İŞLEMLERİ ----------------

    // Doktor listesi
    public async Task<IActionResult> Doktorlar()
    {
        var doktorlar = await _context.Doktorlar
            .Include(d => d.Poliklinik)
            .Include(d => d.Kullanici)
            .OrderBy(d => d.Poliklinik.Ad)
            .ThenBy(d => d.Ad)
            .ThenBy(d => d.Soyad)
            .ToListAsync();

        return View(doktorlar);
    }

    // Doktor ekleme sayfası
    [HttpGet]
    public async Task<IActionResult> DoktorEkle()
    {
        await PoliklinikListesiniHazirla();

        return View(new DoktorEkleViewModel());
    }

    // Doktor ve giriş hesabı oluşturma
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoktorEkle(
        DoktorEkleViewModel model)
    {
        model.Ad = model.Ad?.Trim() ?? "";
        model.Soyad = model.Soyad?.Trim() ?? "";
        model.Unvan = model.Unvan?.Trim() ?? "";
        model.Email = model.Email?.Trim() ?? "";

        if (!ModelState.IsValid)
        {
            await PoliklinikListesiniHazirla();
            return View(model);
        }

        var poliklinik = await _context.Poliklinikler
            .FirstOrDefaultAsync(p =>
                p.Id == model.PoliklinikId && p.AktifMi);

        if (poliklinik == null)
        {
            ModelState.AddModelError(
                nameof(model.PoliklinikId),
                "Geçerli ve aktif bir poliklinik seçmelisiniz.");

            await PoliklinikListesiniHazirla();
            return View(model);
        }

        var mevcutKullanici =
            await _userManager.FindByEmailAsync(model.Email);

        if (mevcutKullanici != null)
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "Bu e-posta adresi zaten kayıtlı.");

            await PoliklinikListesiniHazirla();
            return View(model);
        }

        // Kullanıcı hesabı ve doktor kaydı aynı işlemde oluşturulur.
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var kullanici = new ApplicationUser
        {
            Ad = model.Ad,
            Soyad = model.Soyad,
            Email = model.Email,
            UserName = model.Email,
            EmailConfirmed = true
        };

        var kullaniciSonucu = await _userManager.CreateAsync(
            kullanici,
            model.Parola);

        if (!kullaniciSonucu.Succeeded)
        {
            await transaction.RollbackAsync();

            foreach (var error in kullaniciSonucu.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            await PoliklinikListesiniHazirla();
            return View(model);
        }

        var rolSonucu = await _userManager.AddToRoleAsync(
            kullanici,
            "Doktor");

        if (!rolSonucu.Succeeded)
        {
            await transaction.RollbackAsync();

            foreach (var error in rolSonucu.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            await PoliklinikListesiniHazirla();
            return View(model);
        }

        var doktor = new Doktor
        {
            Unvan = model.Unvan,
            Ad = model.Ad,
            Soyad = model.Soyad,
            PoliklinikId = model.PoliklinikId,
            AktifMi = true,
            KullaniciId = kullanici.Id
        };

        _context.Doktorlar.Add(doktor);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["Basari"] =
            "Doktor ve giriş hesabı başarıyla oluşturuldu.";

        return RedirectToAction(nameof(Doktorlar));
    }

    // Doktor düzenleme sayfası
    [HttpGet]
    public async Task<IActionResult> DoktorDuzenle(int id)
    {
        var doktor = await _context.Doktorlar
            .Include(d => d.Kullanici)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doktor == null)
            return NotFound();

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

    // Doktor bilgilerini güncelleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoktorDuzenle(
        DoktorDuzenleViewModel model)
    {
        model.Ad = model.Ad?.Trim() ?? "";
        model.Soyad = model.Soyad?.Trim() ?? "";
        model.Unvan = model.Unvan?.Trim() ?? "";

        var doktor = await _context.Doktorlar
            .Include(d => d.Kullanici)
            .FirstOrDefaultAsync(d => d.Id == model.Id);

        if (doktor == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.HesapEpostasi = doktor.Kullanici?.Email;

            await PoliklinikListesiniHazirla(
                model.PoliklinikId,
                pasifleriDahilEt: true);

            return View(model);
        }

        bool poliklinikMevcut = await _context.Poliklinikler
            .AnyAsync(p => p.Id == model.PoliklinikId);

        if (!poliklinikMevcut)
        {
            ModelState.AddModelError(
                nameof(model.PoliklinikId),
                "Seçilen poliklinik bulunamadı.");

            model.HesapEpostasi = doktor.Kullanici?.Email;

            await PoliklinikListesiniHazirla(
                model.PoliklinikId,
                pasifleriDahilEt: true);

            return View(model);
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        // Doktorun giriş hesabı varsa ad ve soyad bilgilerini de güncelle.
        if (doktor.Kullanici != null)
        {
            doktor.Kullanici.Ad = model.Ad;
            doktor.Kullanici.Soyad = model.Soyad;

            var kullaniciGuncellemeSonucu =
                await _userManager.UpdateAsync(doktor.Kullanici);

            if (!kullaniciGuncellemeSonucu.Succeeded)
            {
                await transaction.RollbackAsync();

                foreach (var error in kullaniciGuncellemeSonucu.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                model.HesapEpostasi = doktor.Kullanici.Email;

                await PoliklinikListesiniHazirla(
                    model.PoliklinikId,
                    pasifleriDahilEt: true);

                return View(model);
            }
        }

        doktor.Unvan = model.Unvan;
        doktor.Ad = model.Ad;
        doktor.Soyad = model.Soyad;
        doktor.PoliklinikId = model.PoliklinikId;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["Basari"] = "Doktor bilgileri güncellendi.";

        return RedirectToAction(nameof(Doktorlar));
    }

    // Doktoru aktif veya pasif yapma
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoktorDurumDegistir(int id)
    {
        var doktor = await _context.Doktorlar
            .FindAsync(id);

        if (doktor == null)
            return NotFound();

        doktor.AktifMi = !doktor.AktifMi;

        await _context.SaveChangesAsync();

        TempData["Basari"] = "Doktor durumu güncellendi.";

        return RedirectToAction(nameof(Doktorlar));
    }

    // Doktor formlarındaki poliklinik seçeneklerini hazırlar.
    private async Task PoliklinikListesiniHazirla(
        int? seciliId = null,
        bool pasifleriDahilEt = false)
    {
        var sorgu = _context.Poliklinikler.AsQueryable();

        if (!pasifleriDahilEt)
        {
            sorgu = sorgu.Where(p => p.AktifMi);
        }

        ViewBag.Poliklinikler = await sorgu
            .OrderBy(p => p.Ad)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Ad + (p.AktifMi ? "" : " (Pasif)"),
                Selected = p.Id == seciliId
            })
            .ToListAsync();
    }
}