using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Services;

public class KurumService : IKurumService
{
    private readonly AppDbContext _context;

    private static readonly CultureInfo TurkceKultur =
        CultureInfo.GetCultureInfo("tr-TR");

    public KurumService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Kurum>> ListeleAsync()
    {
        return await _context.Kurumlar
            .AsNoTracking()
            .OrderBy(k => k.Ad)
            .ToListAsync();
    }

    public async Task<Kurum?> GetirAsync(int id)
    {
        return await _context.Kurumlar
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task EkleAsync(
        string ad,
        string? aciklama,
        bool aktifMi)
    {
        var kurum = new Kurum(ad, aciklama, aktifMi);

        await AyniAdKontrolEtAsync(kurum.Ad);

        _context.Kurumlar.Add(kurum);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> GuncelleAsync(
        int id,
        string ad,
        string? aciklama,
        bool aktifMi)
    {
        var kurum = await _context.Kurumlar
            .FirstOrDefaultAsync(k => k.Id == id);

        if (kurum is null)
        {
            return false;
        }

        var temizAd = ad?.Trim() ?? string.Empty;

        await AyniAdKontrolEtAsync(temizAd, id);

        kurum.BilgileriGuncelle(temizAd, aciklama);
        kurum.AktiflikAyarla(aktifMi);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DurumDegistirAsync(int id)
    {
        var kurum = await _context.Kurumlar
            .FirstOrDefaultAsync(k => k.Id == id);

        if (kurum is null)
        {
            return false;
        }

        kurum.DurumDegistir();

        await _context.SaveChangesAsync();

        return true;
    }

    private async Task AyniAdKontrolEtAsync(
        string ad,
        int? haricTutulanId = null)
    {
        var karsilastirilacakAd = AdiNormallestir(ad);

        var sorgu = _context.Kurumlar
            .AsNoTracking()
            .AsQueryable();

        if (haricTutulanId.HasValue)
        {
            sorgu = sorgu.Where(
                k => k.Id != haricTutulanId.Value);
        }

        // Pasif kurumlar da aynı isim kontrolüne dahildir.
        var mevcutAdlar = await sorgu
            .Select(k => k.Ad)
            .ToListAsync();

        var mevcut = mevcutAdlar.Any(mevcutAd =>
            string.Equals(
                AdiNormallestir(mevcutAd),
                karsilastirilacakAd,
                StringComparison.Ordinal));

        if (mevcut)
        {
            throw new ArgumentException(
                "Bu isimde bir kurum zaten mevcut.");
        }
    }

    private static string AdiNormallestir(string ad)
    {
        var sonuc = ad.Trim();

        sonuc = sonuc.Normalize(NormalizationForm.FormC);
        sonuc = Regex.Replace(sonuc, @"\s+", " ");
        sonuc = sonuc.ToUpper(TurkceKultur);

        return sonuc.Normalize(NormalizationForm.FormC);
    }
}