using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Services;

public class RandevuSecimService : IRandevuSecimService
{
    private readonly AppDbContext _context;

    private const int IleriTarihSiniriGun = 30;

    private static readonly TimeZoneInfo TurkiyeSaatDilimi =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

    public RandevuSecimService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RandevuSecimViewModel> HazirlaAsync(
        int? kurumId = null,
        int? subeId = null,
        int? poliklinikId = null,
        int? doktorId = null,
        DateOnly? tarih = null)
    {
        // Seçim yapılmayan alanlarda 0 veya null kullanılabilir.
        kurumId = SecimiDuzenle(kurumId);
        subeId = SecimiDuzenle(subeId);
        poliklinikId = SecimiDuzenle(poliklinikId);
        doktorId = SecimiDuzenle(doktorId);

        var simdi = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            TurkiyeSaatDilimi);

        var bugun = DateOnly.FromDateTime(simdi);
        var sonTarih = bugun.AddDays(IleriTarihSiniriGun);

        if (tarih.HasValue)
        {
            if (!doktorId.HasValue)
            {
                throw new ArgumentException(
                    "Tarih seçmeden önce bir doktor seçmelisiniz.");
            }

            if (tarih.Value < bugun || tarih.Value > sonTarih)
            {
                throw new ArgumentException(
                    "Bugünden itibaren en fazla 30 gün sonrası için " +
                    "tarih seçebilirsiniz.");
            }
        }

        // Ana sayfadan yalnızca poliklinikId ile gelinmesini destekler.
        if (poliklinikId.HasValue)
        {
            var poliklinik = await _context.Poliklinikler
                .AsNoTracking()
                .Include(p => p.Sube)
                    .ThenInclude(s => s!.Kurum)
                .FirstOrDefaultAsync(
                    p => p.Id == poliklinikId.Value);

            if (poliklinik is null
                || !poliklinik.AktifMi
                || poliklinik.Sube is null
                || !poliklinik.Sube.AktifMi
                || poliklinik.Sube.Kurum is null
                || !poliklinik.Sube.Kurum.AktifMi)
            {
                throw new ArgumentException(
                    "Seçilen poliklinik veya bağlı olduğu hastane ve kurum " +
                    "randevu seçimine uygun değil. Lütfen yeniden seçim yapın.");
            }

            if (subeId.HasValue
                && subeId.Value != poliklinik.Sube.Id)
            {
                throw new ArgumentException(
                    "Seçilen poliklinik bu hastaneye bağlı değil.");
            }

            if (kurumId.HasValue
                && kurumId.Value != poliklinik.Sube.KurumId)
            {
                throw new ArgumentException(
                    "Seçilen poliklinik bu kuruma bağlı değil.");
            }

            subeId = poliklinik.Sube.Id;
            kurumId = poliklinik.Sube.KurumId;
        }

        if (subeId.HasValue)
        {
            var sube = await _context.Subeler
                .AsNoTracking()
                .Include(s => s.Kurum)
                .FirstOrDefaultAsync(s => s.Id == subeId.Value);

            if (sube is null
                || !sube.AktifMi
                || sube.Kurum is null
                || !sube.Kurum.AktifMi)
            {
                throw new ArgumentException(
                    "Seçilen hastane veya bağlı olduğu kurum " +
                    "randevu seçimine uygun değil. Lütfen yeniden seçim yapın.");
            }

            if (kurumId.HasValue && kurumId.Value != sube.KurumId)
            {
                throw new ArgumentException(
                    "Seçilen hastane bu kuruma bağlı değil.");
            }

            kurumId = sube.KurumId;
        }

        if (doktorId.HasValue && !poliklinikId.HasValue)
        {
            throw new ArgumentException(
                "Doktor seçmeden önce bir poliklinik seçmelisiniz.");
        }

        var model = new RandevuSecimViewModel
        {
            Bugun = bugun,
            SonSecilebilirTarih = sonTarih,

            Kurumlar = await _context.Kurumlar
                .AsNoTracking()
                .Where(k => k.AktifMi)
                .OrderBy(k => k.Ad)
                .ToListAsync()
        };

        // İlk açılışta yalnızca kurum seçenekleri hazırlanır.
        if (!kurumId.HasValue)
        {
            return model;
        }

        if (!model.Kurumlar.Any(k => k.Id == kurumId.Value))
        {
            throw new ArgumentException(
                "Seçilen kurum bulunamadı veya pasif durumda.");
        }

        model.SeciliKurumId = kurumId.Value;

        model.Subeler = await _context.Subeler
            .AsNoTracking()
            .Where(s =>
                s.KurumId == kurumId.Value
                && s.AktifMi
                && s.Kurum.AktifMi)
            .OrderBy(s => s.Ad)
            .ToListAsync();

        if (!subeId.HasValue)
        {
            return model;
        }

        if (!model.Subeler.Any(s => s.Id == subeId.Value))
        {
            throw new ArgumentException(
                "Seçilen hastane bu kurumda bulunamadı veya pasif durumda.");
        }

        model.SeciliSubeId = subeId.Value;

        model.Poliklinikler = await _context.Poliklinikler
            .AsNoTracking()
            .Where(p =>
                p.SubeId == subeId.Value
                && p.AktifMi
                && p.Sube != null
                && p.Sube.AktifMi
                && p.Sube.Kurum.AktifMi)
            .OrderBy(p => p.Ad)
            .ToListAsync();

        if (!poliklinikId.HasValue)
        {
            return model;
        }

        if (!model.Poliklinikler.Any(
                p => p.Id == poliklinikId.Value))
        {
            throw new ArgumentException(
                "Seçilen poliklinik bu hastanede bulunamadı veya pasif durumda.");
        }

        model.SeciliPoliklinikId = poliklinikId.Value;

        model.Doktorlar = await _context.Doktorlar
            .AsNoTracking()
            .Include(d => d.Poliklinik)
            .Where(d =>
                d.PoliklinikId == poliklinikId.Value
                && d.AktifMi
                && d.Poliklinik.AktifMi
                && d.Poliklinik.Sube != null
                && d.Poliklinik.Sube.AktifMi
                && d.Poliklinik.Sube.Kurum.AktifMi)
            .OrderBy(d => d.Ad)
            .ThenBy(d => d.Soyad)
            .ToListAsync();

        if (!doktorId.HasValue)
        {
            return model;
        }

        var doktor = model.Doktorlar
            .FirstOrDefault(d => d.Id == doktorId.Value);

        if (doktor is null)
        {
            throw new ArgumentException(
                "Seçilen doktor bu poliklinikte bulunamadı " +
                "veya randevu seçimine uygun değil.");
        }

        model.SeciliDoktorId = doktor.Id;
        model.SeciliDoktor = doktor;

        // Doktor ilk seçildiğinde bugünün programı gösterilir.
        model.SeciliTarih = tarih ?? bugun;

        await SaatleriHazirlaAsync(model, simdi);

        return model;
    }

    private async Task SaatleriHazirlaAsync(
        RandevuSecimViewModel model,
        DateTime simdi)
    {
        var tarih = model.SeciliTarih!.Value;
        var doktorId = model.SeciliDoktorId!.Value;
        var gun = tarih.DayOfWeek;

        var programlar = await _context.DoktorCalismaSaatleri
            .AsNoTracking()
            .Where(c =>
                c.DoktorId == doktorId
                && c.Gun == gun
                && c.AktifMi)
            .OrderBy(c => c.BaslangicSaati)
            .ToListAsync();

        model.SeciliGundeCalismaVar = programlar.Count > 0;

        var suAnkiSaat = TimeOnly.FromDateTime(simdi);

        foreach (var program in programlar)
        {
            var baslangic = program.BaslangicSaati.ToTimeSpan();
            var bitis = program.BitisSaati.ToTimeSpan();

            var sure = TimeSpan.FromMinutes(
                program.RandevuSuresiDakika);

            // Yalnızca tamamı çalışma aralığına sığan randevuları üretiriz.
            // TimeSpan hesabı gece yarısında saatin başa dönmesini önler.
            for (
                var saat = baslangic;
                saat + sure <= bitis;
                saat += sure)
            {
                var baslangicSaati = TimeOnly.FromTimeSpan(saat);

                // Bugün için başlamış veya geçmiş saatler gösterilmez.
                if (tarih == model.Bugun
                    && baslangicSaati <= suAnkiSaat)
                {
                    continue;
                }

                model.Saatler.Add(new RandevuSaatViewModel
                {
                    BaslangicSaati = baslangicSaati,
                    BitisSaati = TimeOnly.FromTimeSpan(saat + sure),
                    RandevuSuresiDakika = program.RandevuSuresiDakika
                });
            }
        }
    }

    private static int? SecimiDuzenle(int? id)
    {
        if (id is < 0)
        {
            throw new ArgumentException(
                "Geçersiz bir seçim gönderildi. Lütfen yeniden seçim yapın.");
        }

        return id == 0 ? null : id;
    }
}