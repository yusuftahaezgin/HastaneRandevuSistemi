
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers
{
    [Authorize(Roles = "Yonetici")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Poliklinik listesi
        public async Task<IActionResult> Poliklinikler()
        {
            var poliklinikler = await _context.Poliklinikler
                .OrderBy(p => p.Ad)
                .ToListAsync();

            return View(poliklinikler);
        }

        // Poliklinik ekleme sayfası
        [HttpGet]
        public IActionResult PoliklinikEkle()
        {
            return View();
        }

        // Poliklinik ekleme işlemi
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

        // Poliklinik düzenleme sayfası
        [HttpGet]
        public async Task<IActionResult> PoliklinikDuzenle(int id)
        {
            var poliklinik = await _context.Poliklinikler
                .FindAsync(id);

            if (poliklinik == null)
                return NotFound();

            return View(poliklinik);
        }

        // Poliklinik düzenleme işlemi
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

        // Poliklinik aktif / pasif yapma
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
    }
}
