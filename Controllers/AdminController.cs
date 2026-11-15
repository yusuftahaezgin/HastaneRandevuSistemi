using System.Globalization;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services;
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
    private readonly IDoktorCalismaSaatiService _calismaSaatiService;
    private readonly RandevuKayitService _randevuKayitService;
    private readonly HastaneCalismaService _hastaneCalismaService;

    public AdminController(
        IPoliklinikService poliklinikService,
        IDoktorService doktorService,
        ISaglikPersoneliService personelService,
        IKurumService kurumService,
        ISubeService subeService,
        IDoktorCalismaSaatiService calismaSaatiService,
        AppDbContext db)
    {
        _poliklinikService = poliklinikService;
        _doktorService = doktorService;
        _personelService = personelService;
        _kurumService = kurumService;
        _subeService = subeService;
        _calismaSaatiService = calismaSaatiService;
        _randevuKayitService = new RandevuKayitService(db);
        _hastaneCalismaService = new HastaneCalismaService(db);
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
        return View(await _kurumService.ListeleAsync());
    }

    [HttpGet]
    public IActionResult KurumEkle()
    {
        return View(new KurumFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KurumEkle(KurumFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _kurumService.EkleAsync(
                model.Ad, model.Aciklama, model.AktifMi);
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
            return NotFound();

        return View(new KurumFormViewModel
        {
            Id = kurum.Id,
            Ad = kurum.Ad,
            Aciklama = kurum.Aciklama,
            AktifMi = kurum.AktifMi
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KurumDuzenle(
        int id, KurumFormViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        var kurum = await _kurumService.GetirAsync(id);

        if (kurum is null)
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var guncellendi = await _kurumService.GuncelleAsync(
                id, model.Ad, model.Aciklama, model.AktifMi);

            if (!guncellendi)
                return NotFound();
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
        if (!await _kurumService.DurumDegistirAsync(id))
            return NotFound();

        TempData["Basari"] = "Kurumun aktiflik durumu güncellendi.";
        return RedirectToAction(nameof(Kurumlar));
    }

    // ŞUBE İŞLEMLERİ

    [HttpGet]
    public async Task<IActionResult> Subeler()
    {
        return View(await _subeService.ListeleAsync());
    }

    [HttpGet]
    public async Task<IActionResult> SubeEkle()
    {
        await KurumListesiniHazirla();
        return View(new SubeFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubeEkle(SubeFormViewModel model)
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
            return NotFound();

        ViewBag.KurumAdi = sube.Kurum.AktifMi
            ? sube.Kurum.Ad
            : $"{sube.Kurum.Ad} (Pasif)";

        return View(new SubeFormViewModel
        {
            Id = sube.Id,
            KurumId = sube.KurumId,
            Ad = sube.Ad,
            Adres = sube.Adres,
            Telefon = sube.Telefon,
            AktifMi = sube.AktifMi
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubeDuzenle(
        int id, SubeFormViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        var sube = await _subeService.GetirAsync(id);

        if (sube is null)
            return NotFound();

        if (model.KurumId != sube.KurumId)
            return BadRequest();

        ViewBag.KurumAdi = sube.Kurum.AktifMi
            ? sube.Kurum.Ad
            : $"{sube.Kurum.Ad} (Pasif)";

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var guncellendi = await _subeService.GuncelleAsync(
                id,
                model.Ad,
                model.Adres,
                model.Telefon,
                model.AktifMi);

            if (!guncellendi)
                return NotFound();
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
        if (!await _subeService.DurumDegistirAsync(id))
            return NotFound();

        TempData["Basari"] = "Şubenin aktiflik durumu güncellendi.";
        return RedirectToAction(nameof(Subeler));
    }

    // POLİKLİNİK İŞLEMLERİ

    [HttpGet]
    public async Task<IActionResult> Poliklinikler()
    {
        return View(await _poliklinikService.ListeleAsync());
    }

    [HttpGet]
    public async Task<IActionResult> PoliklinikEkle()
    {
        await PoliklinikIcinSubeListesiniHazirla();
        return View(new PoliklinikFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PoliklinikEkle(
        PoliklinikFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PoliklinikIcinSubeListesiniHazirla(model.SubeId);
            return View(model);
        }

        try
        {
            await _poliklinikService.EkleAsync(
                model.SubeId,
                model.Ad,
                model.Aciklama,
                model.AktifMi);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PoliklinikIcinSubeListesiniHazirla(model.SubeId);
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
            return NotFound();

        var model = new PoliklinikFormViewModel
        {
            Id = poliklinik.Id,
            SubeId = poliklinik.SubeId ?? 0,
            Ad = poliklinik.Ad,
            Aciklama = poliklinik.Aciklama,
            AktifMi = poliklinik.AktifMi
        };

        await PoliklinikIcinSubeListesiniHazirla(
            model.SubeId, poliklinik.SubeId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PoliklinikDuzenle(
        int id, PoliklinikFormViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        var poliklinik = await _poliklinikService.GetirAsync(id);

        if (poliklinik is null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            await PoliklinikIcinSubeListesiniHazirla(
                model.SubeId, poliklinik.SubeId);
            return View(model);
        }

        try
        {
            var guncellendi = await _poliklinikService.GuncelleAsync(
                id,
                model.SubeId,
                model.Ad,
                model.Aciklama,
                model.AktifMi);

            if (!guncellendi)
                return NotFound();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PoliklinikIcinSubeListesiniHazirla(
                model.SubeId, poliklinik.SubeId);
            return View(model);
        }

        TempData["Basari"] = "Poliklinik başarıyla güncellendi.";
        return RedirectToAction(nameof(Poliklinikler));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DurumDegistir(int id)
    {
        if (!await _poliklinikService.DurumDegistirAsync(id))
            return NotFound();

        TempData["Basari"] = "Poliklinik durumu güncellendi.";
        return RedirectToAction(nameof(Poliklinikler));
    }

    // DOKTOR İŞLEMLERİ

    [HttpGet]
    public async Task<IActionResult> Doktorlar()
    {
        return View(await _doktorService.ListeleAsync());
    }

    [HttpGet]
    public async Task<IActionResult> DoktorEkle()
    {
        await PoliklinikListesiniHazirla();
        return View(new DoktorEkleViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoktorEkle(DoktorEkleViewModel model)
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

        TempData["Basari"] = "Doktor ve giriş hesabı başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Doktorlar));
    }

    [HttpGet]
    public async Task<IActionResult> DoktorDuzenle(int id)
    {
        var doktor = await _doktorService.GetirAsync(id);

        if (doktor is null)
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
            model.PoliklinikId, doktor.PoliklinikId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoktorDuzenle(
        DoktorDuzenleViewModel model)
    {
        var doktor = await _doktorService.GetirAsync(model.Id);

        if (doktor is null)
            return NotFound();

        model.HesapEpostasi = doktor.Kullanici?.Email;

        if (!ModelState.IsValid)
        {
            await PoliklinikListesiniHazirla(
                model.PoliklinikId, doktor.PoliklinikId);
            return View(model);
        }

        try
        {
            if (!await _doktorService.GuncelleAsync(model))
                return NotFound();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PoliklinikListesiniHazirla(
                model.PoliklinikId, doktor.PoliklinikId);
            return View(model);
        }

        TempData["Basari"] = "Doktor bilgileri güncellendi.";
        return RedirectToAction(nameof(Doktorlar));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoktorDurumDegistir(int id)
    {
        if (!await _doktorService.DurumDegistirAsync(id))
            return NotFound();

        TempData["Basari"] = "Doktor durumu güncellendi.";
        return RedirectToAction(nameof(Doktorlar));
    }

    // SAĞLIK PERSONELİ İŞLEMLERİ

    [HttpGet]
    public async Task<IActionResult> SaglikPersonelleri()
    {
        return View(await _personelService.ListeleAsync());
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
            return NotFound();

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
            model.PoliklinikId, personel.PoliklinikId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaglikPersoneliDuzenle(
        int id, SaglikPersoneliDuzenleViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        var personel = await _personelService.GetirAsync(id);

        if (personel is null)
            return NotFound();

        model.HesapEpostasi = personel.Kullanici?.Email;

        if (!ModelState.IsValid)
        {
            await PoliklinikListesiniHazirla(
                model.PoliklinikId, personel.PoliklinikId);
            return View(model);
        }

        try
        {
            if (!await _personelService.GuncelleAsync(model))
                return NotFound();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PoliklinikListesiniHazirla(
                model.PoliklinikId, personel.PoliklinikId);
            return View(model);
        }

        TempData["Basari"] = "Sağlık personeli başarıyla güncellendi.";
        return RedirectToAction(nameof(SaglikPersonelleri));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaglikPersoneliDurumDegistir(int id)
    {
        if (!await _personelService.DurumDegistirAsync(id))
            return NotFound();

        TempData["Basari"] = "Personelin aktiflik durumu güncellendi.";
        return RedirectToAction(nameof(SaglikPersonelleri));
    }

    // DOKTOR ÇALIŞMA SAATLERİ

    [HttpGet]
    public async Task<IActionResult> CalismaSaatleri()
    {
        return View(await _calismaSaatiService.ListeleAsync());
    }

    [HttpGet]
    public async Task<IActionResult> CalismaSaatiEkle()
    {
        await CalismaSaatiDoktorlariniHazirla();
        CalismaGunleriniHazirla();

        return View(new DoktorCalismaSaatiFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CalismaSaatiEkle(
        DoktorCalismaSaatiFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _calismaSaatiService.EkleAsync(model);
                TempData["Basari"] = "Çalışma saati başarıyla eklendi.";
                return RedirectToAction(nameof(CalismaSaatleri));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        await CalismaSaatiDoktorlariniHazirla(model.DoktorId);
        CalismaGunleriniHazirla(model.Gun);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> CalismaSaatiDuzenle(int id)
    {
        var kayit = await _calismaSaatiService.GetirAsync(id);

        if (kayit is null)
            return NotFound();

        var model = new DoktorCalismaSaatiFormViewModel
        {
            Id = kayit.Id,
            DoktorId = kayit.DoktorId,
            Gun = kayit.Gun,
            BaslangicSaati = kayit.BaslangicSaati,
            BitisSaati = kayit.BitisSaati,
            RandevuSuresiDakika = kayit.RandevuSuresiDakika,
            AktifMi = kayit.AktifMi
        };

        ViewBag.DoktorBilgisi = CalismaSaatiDoktorMetni(kayit.Doktor);
        CalismaGunleriniHazirla(model.Gun);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CalismaSaatiDuzenle(
        int id, DoktorCalismaSaatiFormViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        var kayit = await _calismaSaatiService.GetirAsync(id);

        if (kayit is null)
            return NotFound();

        if (model.DoktorId != kayit.DoktorId)
            return BadRequest();

        if (ModelState.IsValid)
        {
            try
            {
                if (!await _calismaSaatiService.GuncelleAsync(model))
                    return NotFound();

                TempData["Basari"] = "Çalışma saati başarıyla güncellendi.";
                return RedirectToAction(nameof(CalismaSaatleri));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        ViewBag.DoktorBilgisi = CalismaSaatiDoktorMetni(kayit.Doktor);
        CalismaGunleriniHazirla(model.Gun);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CalismaSaatiDurumDegistir(int id)
    {
        try
        {
            if (!await _calismaSaatiService.DurumDegistirAsync(id))
                return NotFound();

            TempData["Basari"] =
                "Çalışma saatinin aktiflik durumu güncellendi.";
        }
        catch (ArgumentException ex)
        {
            TempData["Hata"] = ex.Message;
        }

        return RedirectToAction(nameof(CalismaSaatleri));
    }

    // HASTANE ÇALIŞMA SAATLERİ

    [HttpGet]
    public async Task<IActionResult> HastaneSaatleri()
    {
        return View(await _hastaneCalismaService.ListeAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HastaneSaatiKaydet(
        int subeId,
        int gun,
        bool acikMi,
        string baslangic,
        string bitis)
    {
        try
        {
            RandevuFormunuKontrolEt();

            await _hastaneCalismaService.KaydetAsync(
                subeId, gun, acikMi, baslangic, bitis);

            TempData["Basari"] =
                "Hastanenin çalışma saati güncellendi.";
        }
        catch (ArgumentException ex)
        {
            TempData["Hata"] = ex.Message;
        }

        return RedirectToAction(nameof(HastaneSaatleri));
    }

    // ÜCRET VE HİZMET YÖNETİMİ

    [HttpGet]
    public async Task<IActionResult> UcretVeHizmetler()
    {
        return View(await _randevuKayitService.TarifelerAsync());
    }

    // Eski adres, ortak yönetim ekranına yönlenir.
    [HttpGet]
    public IActionResult DoktorHizmetleri()
    {
        return RedirectToAction(nameof(UcretVeHizmetler));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoktorHizmetiKaydet(
        int id,
        string hizmetAdi,
        string ucret)
    {
        try
        {
            RandevuFormunuKontrolEt();

            await _randevuKayitService.DoktorUcretiAsync(
                id,
                RandevuKayitService.UcretOku(ucret),
                hizmetAdi);

            TempData["Basari"] =
                "Doktorun hizmet adı ve ücreti güncellendi.";
        }
        catch (ArgumentException ex)
        {
            TempData["Hata"] = ex.Message;
        }

        return RedirectToAction(nameof(UcretVeHizmetler));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoktorUcreti(int id, string ucret)
    {
        try
        {
            RandevuFormunuKontrolEt();

            await _randevuKayitService.DoktorUcretiAsync(
                id, RandevuKayitService.UcretOku(ucret));

            TempData["Basari"] = "Doktor ücreti güncellendi.";
        }
        catch (ArgumentException ex)
        {
            TempData["Hata"] = ex.Message;
        }

        return RedirectToAction(nameof(UcretVeHizmetler));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PersonelHizmetiKaydet(
        int id,
        int personelId,
        string ad,
        int sure,
        string ucret,
        bool aktif)
    {
        try
        {
            RandevuFormunuKontrolEt();

            await _randevuKayitService.HizmetKaydetAsync(
                id,
                personelId,
                ad,
                sure,
                RandevuKayitService.UcretOku(ucret),
                aktif);

            TempData["Basari"] = "Hizmet kaydedildi.";
        }
        catch (ArgumentException ex)
        {
            TempData["Hata"] = ex.Message;
        }

        return RedirectToAction(nameof(UcretVeHizmetler));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PersonelCalismaSaatiEkle(
        int personelId,
        int gun,
        string baslangic,
        string bitis)
    {
        try
        {
            RandevuFormunuKontrolEt();

            if (!TimeOnly.TryParseExact(
                    baslangic,
                    "HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var baslangicSaati)
                || !TimeOnly.TryParseExact(
                    bitis,
                    "HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var bitisSaati))
            {
                throw new ArgumentException(
                    "Geçerli başlangıç ve bitiş saati girin.");
            }

            await _randevuKayitService.SaatEkleAsync(
                personelId, gun, baslangicSaati, bitisSaati);

            TempData["Basari"] = "Personel çalışma saati eklendi.";
        }
        catch (ArgumentException ex)
        {
            TempData["Hata"] = ex.Message;
        }

        return RedirectToAction(nameof(UcretVeHizmetler));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PersonelCalismaSaatiSil(int id)
    {
        try
        {
            RandevuFormunuKontrolEt();
            await _randevuKayitService.SaatSilAsync(id);
            TempData["Basari"] = "Çalışma saati kaldırıldı.";
        }
        catch (ArgumentException ex)
        {
            TempData["Hata"] = ex.Message;
        }

        return RedirectToAction(nameof(UcretVeHizmetler));
    }

    // RANDEVU ONAYLARI

    [HttpGet]
    public async Task<IActionResult> Randevular()
    {
        var randevular = await _randevuKayitService.ListeAsync();
        return View("~/Views/Randevu/Liste.cshtml", randevular);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RandevuKarar(int id, string karar)
    {
        try
        {
            RandevuFormunuKontrolEt();
            await _randevuKayitService.AdminKararAsync(id, karar);
            TempData["Basari"] = "Randevu durumu güncellendi.";
        }
        catch (ArgumentException ex)
        {
            TempData["Hata"] = ex.Message;
        }

        return RedirectToAction(nameof(Randevular));
    }

    private void RandevuFormunuKontrolEt()
    {
        if (!ModelState.IsValid)
        {
            throw new ArgumentException(
                "Gönderilen form bilgileri geçersiz.");
        }
    }

    // FORM SEÇENEKLERİ

    private async Task CalismaSaatiDoktorlariniHazirla(
        int? seciliId = null)
    {
        var doktorlar =
            await _calismaSaatiService.SecilebilirDoktorlariListeleAsync();

        ViewBag.Doktorlar = doktorlar.Select(d => new SelectListItem
        {
            Value = d.Id.ToString(),
            Text = CalismaSaatiDoktorMetni(d),
            Selected = d.Id == seciliId
        }).ToList();

        ViewBag.SecilebilirDoktorVar = doktorlar.Count > 0;
    }

    private void CalismaGunleriniHazirla(DayOfWeek? seciliGun = null)
    {
        var gunler = new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        };

        var kultur = CultureInfo.GetCultureInfo("tr-TR");

        ViewBag.Gunler = gunler.Select(g => new SelectListItem
        {
            Value = ((int)g).ToString(),
            Text = kultur.DateTimeFormat.GetDayName(g),
            Selected = g == seciliGun
        }).ToList();
    }

    private static string CalismaSaatiDoktorMetni(Doktor doktor)
    {
        return $"{PoliklinikSecenekMetni(doktor.Poliklinik)} / " +
               $"{doktor.GorunenAdGetir()} (ID: {doktor.Id})" +
               (!doktor.AktifMi ? " (Doktor pasif)" : "");
    }

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

    private async Task PoliklinikIcinSubeListesiniHazirla(
        int? seciliId = null,
        int? mevcutSubeId = null)
    {
        var subeler = await _subeService.ListeleAsync();

        var secenekler = subeler
            .Where(s =>
                (s.AktifMi && s.Kurum.AktifMi)
                || s.Id == mevcutSubeId)
            .OrderBy(s => s.Kurum.Ad)
            .ThenBy(s => s.Ad)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Kurum.Ad} / {s.Ad}" +
                    (!s.Kurum.AktifMi ? " (Kurum pasif)" : "") +
                    (!s.AktifMi ? " (Hastane pasif)" : ""),
                Selected = s.Id == seciliId
            })
            .ToList();

        ViewBag.Subeler = secenekler;
        ViewBag.SecilebilirSubeVar = secenekler.Count > 0;
    }

    private async Task PoliklinikListesiniHazirla(
        int? seciliId = null,
        int? mevcutPoliklinikId = null)
    {
        var poliklinikler = await _poliklinikService.ListeleAsync();

        ViewBag.Poliklinikler = poliklinikler
            .Where(p =>
                YeniAtamayaUygunMu(p)
                || p.Id == mevcutPoliklinikId)
            .OrderBy(p => p.Sube?.Ad)
            .ThenBy(p => p.Ad)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = PoliklinikSecenekMetni(p),
                Selected = p.Id == seciliId
            })
            .ToList();
    }

    private static bool YeniAtamayaUygunMu(Poliklinik poliklinik)
    {
        return poliklinik.AktifMi
            && poliklinik.Sube is not null
            && poliklinik.Sube.AktifMi
            && poliklinik.Sube.Kurum is not null
            && poliklinik.Sube.Kurum.AktifMi;
    }

    private static string PoliklinikSecenekMetni(Poliklinik poliklinik)
    {
        var hastaneAdi = poliklinik.Sube?.Ad ?? "Hastane atanmamış";
        var metin = $"{hastaneAdi} / {poliklinik.Ad}";

        if (!poliklinik.AktifMi)
            metin += " (Poliklinik pasif)";

        if (poliklinik.Sube is not null)
        {
            if (!poliklinik.Sube.AktifMi)
                metin += " (Hastane pasif)";

            if (poliklinik.Sube.Kurum is null)
                metin += " (Kurum bağlantısı bulunamadı)";
            else if (!poliklinik.Sube.Kurum.AktifMi)
                metin += " (Kurum pasif)";
        }

        return metin;
    }
}