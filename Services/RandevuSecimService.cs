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

    public async Task<RandevuSecimViewModel?> HazirlaAsync(
        int? poliklinikId,
        int? doktorId)
    {
        var model = new RandevuSecimViewModel
        {
            Poliklinikler = await _context.Poliklinikler
                .AsNoTracking()
                .Where(p => p.AktifMi)
                .OrderBy(p => p.Id)
                .ToListAsync()
        };

        // İlk açılışta yalnızca poliklinik seçenekleri gösterilir.
        if (!poliklinikId.HasValue)
        {
            return model;
        }

        var poliklinik = model.Poliklinikler
            .FirstOrDefault(p => p.Id == poliklinikId.Value);

        // Bulunamayan veya pasif poliklinik seçilemez.
        if (poliklinik is null)
        {
            return null;
        }

        model.SeciliPoliklinikId = poliklinik.Id;

        model.Doktorlar = await _context.Doktorlar
            .AsNoTracking()
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
            // Yalnızca seçilen polikliniğin aktif doktorları seçilebilir.
            model.SeciliDoktor = model.Doktorlar
                .FirstOrDefault(d => d.Id == doktorId.Value);

            model.SeciliDoktorId = model.SeciliDoktor?.Id;
        }

        return model;
    }
}