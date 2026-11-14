using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Services;

public class PoliklinikService : IPoliklinikService
{
    private readonly AppDbContext _context;

    private static readonly CultureInfo TurkceKultur =
        CultureInfo.GetCultureInfo("tr-TR");

    public PoliklinikService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Poliklinik>> ListeleAsync()
    {
        return await _context.Poliklinikler
            .AsNoTracking()
            .Include(p => p.Sube)
                .ThenInclude(s => s!.Kurum)
            .OrderBy(p => p.Sube!.Kurum.Ad)
            .ThenBy(p => p.Sube!.Ad)
            .ThenBy(p => p.Ad)
            .ToListAsync();
    }

    public async Task<Poliklinik?> GetirAsync(int id)
    {
        return await _context.Poliklinikler
            .AsNoTracking()
            .Include(p => p.Sube)
                .ThenInclude(s => s!.Kurum)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task EkleAsync(
        int subeId,
        string ad,
        string? aciklama,
        bool aktifMi)
    {
        await SubeSeciminiKontrolEtAsync(subeId);

        var poliklinik = new Poliklinik(
            ad,
            aciklama,
            aktifMi);

        poliklinik.SubeyeBagla(subeId);

        await AyniAdKontrolEtAsync(
            subeId,
            poliklinik.Ad);

        _context.Poliklinikler.Add(poliklinik);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> GuncelleAsync(
        int id,
        int subeId,
        string ad,
        string? aciklama,
        bool aktifMi)
    {
        var poliklinik = await _context.Poliklinikler
            .FirstOrDefaultAsync(p => p.Id == id);

        if (poliklinik is null)
        {
            return false;
        }

        await SubeSeciminiKontrolEtAsync(
            subeId,
            poliklinik.SubeId);

        var temizAd = ad?.Trim() ?? string.Empty;

        await AyniAdKontrolEtAsync(
            subeId,
            temizAd,
            id);

        poliklinik.BilgileriGuncelle(
            temizAd,
            aciklama);

        poliklinik.SubeyeBagla(subeId);
        poliklinik.AktiflikAyarla(aktifMi);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DurumDegistirAsync(int id)
    {
        var poliklinik = await _context.Poliklinikler
            .FirstOrDefaultAsync(p => p.Id == id);

        if (poliklinik is null)
        {
            return false;
        }

        poliklinik.DurumDegistir();

        await _context.SaveChangesAsync();

        return true;
    }

    private async Task SubeSeciminiKontrolEtAsync(
        int subeId,
        int? mevcutSubeId = null)
    {
        if (subeId <= 0)
        {
            throw new ArgumentException(
                "Lütfen bir hastane seçin.");
        }

        var sube = await _context.Subeler
            .AsNoTracking()
            .Include(s => s.Kurum)
            .FirstOrDefaultAsync(s => s.Id == subeId);

        if (sube is null)
        {
            throw new ArgumentException(
                "Seçilen hastane bulunamadı.");
        }

        // Mevcut hastane sonradan pasif yapılmış olabilir.
        // Hastaneyi değiştirmeden bilgileri düzenlemeye izin verilir.
        if (mevcutSubeId == subeId)
        {
            return;
        }

        if (!sube.Kurum.AktifMi)
        {
            throw new ArgumentException(
                "Seçilen hastanenin bağlı olduğu kurum pasiftir.");
        }

        if (!sube.AktifMi)
        {
            throw new ArgumentException(
                "Pasif bir hastaneye poliklinik eklenemez veya taşınamaz.");
        }
    }

    private async Task AyniAdKontrolEtAsync(
        int subeId,
        string ad,
        int? haricTutulanId = null)
    {
        string karsilastirilacakAd = AdiNormallestir(ad);

        // Karşılaştırma yalnızca seçilen hastane içinde yapılır.
        // Pasif poliklinikler de kontrole dahildir.
        var sorgu = _context.Poliklinikler
            .AsNoTracking()
            .Where(p => p.SubeId == subeId);

        if (haricTutulanId.HasValue)
        {
            sorgu = sorgu.Where(
                p => p.Id != haricTutulanId.Value);
        }

        var mevcutAdlar = await sorgu
            .Select(p => p.Ad)
            .ToListAsync();

        bool mevcut = mevcutAdlar.Any(mevcutAd =>
            string.Equals(
                AdiNormallestir(mevcutAd),
                karsilastirilacakAd,
                StringComparison.Ordinal));

        if (mevcut)
        {
            throw new ArgumentException(
                "Bu hastanede aynı isimde bir poliklinik zaten mevcut.");
        }
    }

    private static string AdiNormallestir(string ad)
    {
        string sonuc = ad.Trim();

        sonuc = sonuc.Normalize(NormalizationForm.FormC);
        sonuc = Regex.Replace(sonuc, @"\s+", " ");
        sonuc = sonuc.ToUpper(TurkceKultur);

        return sonuc.Normalize(NormalizationForm.FormC);
    }
}