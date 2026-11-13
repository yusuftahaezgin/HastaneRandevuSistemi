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
    private readonly ISaglikPersoneliService _personelService;
    private readonly IKurumService _kurumService;
    private readonly ISubeService _subeService;

    public AdminController(
        IPoliklinikService poliklinikService,
        IDoktorService doktorService,
        ISaglikPersoneliService personelService,
        IKurumService kurumService,
        ISubeService subeService)
    {
        _poliklinikService = poliklinikService;
        _doktorService = doktorService;
        _personelService = personelService;
        _kurumService = kurumService;
        _subeService = subeService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // KURUM İŞLEMLERİ

    [HttpGet]
    public async Task<IActionResult> Kurumlar()
    {
        var kurumlar = await _kurumService.ListeleAsync();

        return View(kurumlar);
    }

    [HttpGet]
    public IActionResult KurumEkle()
    {
        return View(new KurumFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KurumEkle(
        KurumFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _kurumService.EkleAsync(
                model.Ad,
                model.Aciklama,
                model.AktifMi);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            return View(model);
        }

        TempData["Basari"] = "Kurum başarıyla eklendi.";

        return RedirectToAction(nameof(Kurumlar));
    }

    [HttpGet]
    public async Task<IActionResult> KurumDuzenle(int id)
    {
        var kurum = await _kurumService.GetirAsync(id);

        if (kurum is null)
        {
            return NotFound();
        }

        var model = new KurumFormViewModel
        {
            Id = kurum.Id,
            Ad = kurum.Ad,
            Aciklama = kurum.Aciklama,
            AktifMi = kurum.AktifMi
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KurumDuzenle(
        int id,
        KurumFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var kurum = await _kurumService.GetirAsync(id);

        if (kurum is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var guncellendi = await _kurumService.GuncelleAsync(
                id,
                model.Ad,
                model.Aciklama,
                model.AktifMi);

            if (!guncellendi)
            {
                return NotFound();
            }
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            return View(model);
        }

        TempData["Basari"] = "Kurum başarıyla güncellendi.";

        return RedirectToAction(nameof(Kurumlar));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KurumDurumDegistir(int id)
    {
        var degisti = await _kurumService.DurumDegistirAsync(id);

        if (!degisti)
        {
            return NotFound();
        }

        TempData["Basari"] = "Kurumun aktiflik durumu güncellendi.";

        return RedirectToAction(nameof(Kurumlar));
    }

    // ŞUBE İŞLEMLERİ

    [HttpGet]
    public async Task<IActionResult> Subeler()
    {
        var subeler = await _subeService.ListeleAsync();

        return View(subeler);
    }

    [HttpGet]
    public async Task<IActionResult> SubeEkle()
    {
        await KurumListesiniHazirla();

        return View(new SubeFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubeEkle(
        SubeFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await KurumListesiniHazirla(model.KurumId);

            return View(model);
        }

        try
        {
            await _subeService.EkleAsync(
                model.KurumId,
                model.Ad,
                model.Adres,
                model.Telefon,
                model.AktifMi);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            await KurumListesiniHazirla(model.KurumId);

            return View(model);
        }

        TempData["Basari"] = "Şube başarıyla eklendi.";

        return RedirectToAction(nameof(Subeler));
    }

    [HttpGet]
    public async Task<IActionResult> SubeDuzenle(int id)
    {
        var sube = await _subeService.GetirAsync(id);

        if (sube is null)
        {
            return NotFound();
        }

        var model = new SubeFormViewModel
        {
            Id = sube.Id,
            KurumId = sube.KurumId,
            Ad = sube.Ad,
            Adres = sube.Adres,
            Telefon = sube.Telefon,
            AktifMi = sube.AktifMi
        };

        ViewBag.KurumAdi = sube.Kurum.AktifMi
            ? sube.Kurum.Ad
            : $"{sube.Kurum.Ad} (Pasif)";

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubeDuzenle(
        int id,
        SubeFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var sube = await _subeService.GetirAsync(id);

        if (sube is null)
        {
            return NotFound();
        }

        // Düzenleme işlemi şubenin bağlı olduğu kurumu değiştirmez.
        if (model.KurumId != sube.KurumId)
        {
            return BadRequest();
        }

        ViewBag.KurumAdi = sube.Kurum.AktifMi
            ? sube.Kurum.Ad
            : $"{sube.Kurum.Ad} (Pasif)";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var guncellendi = await _subeService.GuncelleAsync(
                id,
                model.Ad,
                model.Adres,
                model.Telefon,
                model.AktifMi);

            if (!guncellendi)
            {
                return NotFound();
            }
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            return View(model);
        }

        TempData["Basari"] = "Şube başarıyla güncellendi.";

        return RedirectToAction(nameof(Subeler));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubeDurumDegistir(int id)
    {
        var degisti = await _subeService.DurumDegistirAsync(id);

        if (!degisti)
        {
            return NotFound();
        }

        TempData["Basari"] = "Şubenin aktiflik durumu güncellendi.";

        return RedirectToAction(nameof(Subeler));
    }

    // POLİKLİNİK İŞLEMLERİ

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
            var guncellendi = await _poliklinikService.GuncelleAsync(
                model.Id,
                model.Ad,
                model.Aciklama,
                model.AktifMi);

            if (!guncellendi)
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
        var degisti = await _poliklinikService.DurumDegistirAsync(id);

        if (!degisti)
        {
            return NotFound();
        }

        TempData["Basari"] = "Poliklinik durumu güncellendi.";

        return RedirectToAction(nameof(Poliklinikler));
    }

    // DOKTOR İŞLEMLERİ

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
            model.PoliklinikId,
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
            var guncellendi = await _doktorService.GuncelleAsync(model);

            if (!guncellendi)
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
        var degisti = await _doktorService.DurumDegistirAsync(id);

        if (!degisti)
        {
            return NotFound();
        }

        TempData["Basari"] = "Doktor durumu güncellendi.";

        return RedirectToAction(nameof(Doktorlar));
    }

    // SAĞLIK PERSONELİ İŞLEMLERİ

    [HttpGet]
    public async Task<IActionResult> SaglikPersonelleri()
    {
        var personeller = await _personelService.ListeleAsync();

        return View(personeller);
    }

    [HttpGet]
    public async Task<IActionResult> SaglikPersoneliEkle()
    {
        await PoliklinikListesiniHazirla();

        return View(new SaglikPersoneliEkleViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaglikPersoneliEkle(
        SaglikPersoneliEkleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PoliklinikListesiniHazirla(model.PoliklinikId);

            return View(model);
        }

        try
        {
            await _personelService.EkleAsync(model);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            await PoliklinikListesiniHazirla(model.PoliklinikId);

            return View(model);
        }

        TempData["Basari"] =
            "Sağlık personeli ve giriş hesabı başarıyla oluşturuldu.";

        return RedirectToAction(nameof(SaglikPersonelleri));
    }

    [HttpGet]
    public async Task<IActionResult> SaglikPersoneliDuzenle(int id)
    {
        var personel = await _personelService.GetirAsync(id);

        if (personel is null)
        {
            return NotFound();
        }

        var model = new SaglikPersoneliDuzenleViewModel
        {
            Id = personel.Id,
            Gorev = personel.Gorev,
            Ad = personel.Ad,
            Soyad = personel.Soyad,
            PoliklinikId = personel.PoliklinikId,
            HesapEpostasi = personel.Kullanici?.Email
        };

        await PoliklinikListesiniHazirla(
            model.PoliklinikId,
            mevcutPoliklinikId: personel.PoliklinikId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaglikPersoneliDuzenle(
        int id,
        SaglikPersoneliDuzenleViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var personel = await _personelService.GetirAsync(id);

        if (personel is null)
        {
            return NotFound();
        }

        model.HesapEpostasi = personel.Kullanici?.Email;

        if (!ModelState.IsValid)
        {
            await PoliklinikListesiniHazirla(
                model.PoliklinikId,
                mevcutPoliklinikId: personel.PoliklinikId);

            return View(model);
        }

        try
        {
            var guncellendi = await _personelService.GuncelleAsync(model);

            if (!guncellendi)
            {
                return NotFound();
            }
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            await PoliklinikListesiniHazirla(
                model.PoliklinikId,
                mevcutPoliklinikId: personel.PoliklinikId);

            return View(model);
        }

        TempData["Basari"] = "Sağlık personeli başarıyla güncellendi.";

        return RedirectToAction(nameof(SaglikPersonelleri));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaglikPersoneliDurumDegistir(int id)
    {
        var degisti = await _personelService.DurumDegistirAsync(id);

        if (!degisti)
        {
            return NotFound();
        }

        TempData["Basari"] = "Personelin aktiflik durumu güncellendi.";

        return RedirectToAction(nameof(SaglikPersonelleri));
    }

    // FORM SAYFALARI İÇİN KURUM SEÇENEKLERİ

    private async Task KurumListesiniHazirla(int? seciliId = null)
    {
        var kurumlar = await _kurumService.ListeleAsync();

        var secenekler = kurumlar
            .Where(k => k.AktifMi)
            .OrderBy(k => k.Ad)
            .Select(k => new SelectListItem
            {
                Value = k.Id.ToString(),
                Text = k.Ad,
                Selected = k.Id == seciliId
            })
            .ToList();

        ViewBag.Kurumlar = secenekler;
        ViewBag.AktifKurumVar = secenekler.Count > 0;
    }

    // FORM SAYFALARI İÇİN POLİKLİNİK SEÇENEKLERİ

    private async Task PoliklinikListesiniHazirla(
        int? seciliId = null,
        bool pasifleriDahilEt = false,
        int? mevcutPoliklinikId = null)
    {
        var poliklinikler = await _poliklinikService.ListeleAsync();

        ViewBag.Poliklinikler = poliklinikler
            .Where(p =>
                pasifleriDahilEt ||
                p.AktifMi ||
                p.Id == mevcutPoliklinikId)
            .OrderBy(p => p.Ad)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.AktifMi ? p.Ad : $"{p.Ad} (Pasif)",
                Selected = p.Id == seciliId
            })
            .ToList();
    }
}