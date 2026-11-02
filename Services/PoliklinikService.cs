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
            .OrderBy(p => p.Ad)
            .ToListAsync();
    }

    public async Task<Poliklinik?> GetirAsync(int id)
    {
        return await _context.Poliklinikler
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task EkleAsync(
        string ad,
        string? aciklama,
        bool aktifMi)
    {
        var poliklinik = new Poliklinik(
            ad,
            aciklama,
            aktifMi);

        await AyniAdKontrolEtAsync(poliklinik.Ad);

        _context.Poliklinikler.Add(poliklinik);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> GuncelleAsync(
        int id,
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

        var temizAd = ad?.Trim() ?? string.Empty;

        // Düzenlenen kayıt, kendi adıyla karşılaştırılmaz.
        await AyniAdKontrolEtAsync(temizAd, id);

        poliklinik.BilgileriGuncelle(
            temizAd,
            aciklama);

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

    private async Task AyniAdKontrolEtAsync(
        string ad,
        int? haricTutulanId = null)
    {
        string karsilastirilacakAd = AdiNormallestir(ad);

        var sorgu = _context.Poliklinikler
            .AsNoTracking()
            .AsQueryable();

        if (haricTutulanId.HasValue)
        {
            sorgu = sorgu.Where(
                p => p.Id != haricTutulanId.Value);
        }

        // Yalnızca isimleri getir.
        // Pasif kayıtlar da aynı isim kontrolüne dahildir.
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
                "Bu isimde bir poliklinik zaten mevcut.");
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