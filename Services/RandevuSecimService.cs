using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Services;

public class RandevuSecimService : IRandevuSecimService
{
    private readonly AppDbContext _context;

    public RandevuSecimService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RandevuSecimViewModel> HazirlaAsync(
        int? kurumId = null,
        int? subeId = null,
        int? poliklinikId = null,
        int? doktorId = null)
    {
        // Seçim yapılmayan alanlarda 0 veya null kullanılabilir.
        kurumId = SecimiDuzenle(kurumId);
        subeId = SecimiDuzenle(subeId);
        poliklinikId = SecimiDuzenle(poliklinikId);
        doktorId = SecimiDuzenle(doktorId);

        // Ana sayfadan yalnızca poliklinikId ile gelinmesini destekler.
        if (poliklinikId.HasValue)
        {
            var poliklinik = await _context.Poliklinikler
                .AsNoTracking()
                .Include(p => p.Sube)
                    .ThenInclude(s => s!.Kurum)
                .FirstOrDefaultAsync(p => p.Id == poliklinikId.Value);

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

            if (subeId.HasValue && subeId.Value != poliklinik.Sube.Id)
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

        if (!model.Poliklinikler.Any(p => p.Id == poliklinikId.Value))
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

        return model;
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