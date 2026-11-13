using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Services;

public class SubeService : ISubeService
{
    private readonly AppDbContext _context;

    private static readonly CultureInfo TurkceKultur =
        CultureInfo.GetCultureInfo("tr-TR");

    public SubeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Sube>> ListeleAsync()
    {
        return await _context.Subeler
            .AsNoTracking()
            .Include(s => s.Kurum)
            .OrderBy(s => s.Kurum.Ad)
            .ThenBy(s => s.Ad)
            .ToListAsync();
    }

    public async Task<Sube?> GetirAsync(int id)
    {
        return await _context.Subeler
            .AsNoTracking()
            .Include(s => s.Kurum)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task EkleAsync(
        int kurumId,
        string ad,
        string adres,
        string? telefon,
        bool aktifMi)
    {
        var kurum = await _context.Kurumlar
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == kurumId);

        if (kurum is null)
        {
            throw new ArgumentException(
                "Seçilen kurum bulunamadı.");
        }

        if (!kurum.AktifMi)
        {
            throw new ArgumentException(
                "Pasif bir kuruma yeni şube eklenemez.");
        }

        var sube = new Sube(
            kurumId,
            ad,
            adres,
            telefon,
            aktifMi);

        await AyniAdKontrolEtAsync(kurumId, sube.Ad);

        _context.Subeler.Add(sube);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> GuncelleAsync(
        int id,
        string ad,
        string adres,
        string? telefon,
        bool aktifMi)
    {
        var sube = await _context.Subeler
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sube is null)
        {
            return false;
        }

        var temizAd = ad?.Trim() ?? string.Empty;

        await AyniAdKontrolEtAsync(
            sube.KurumId,
            temizAd,
            id);

        sube.BilgileriGuncelle(
            temizAd,
            adres,
            telefon);

        sube.AktiflikAyarla(aktifMi);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DurumDegistirAsync(int id)
    {
        var sube = await _context.Subeler
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sube is null)
        {
            return false;
        }

        sube.DurumDegistir();

        await _context.SaveChangesAsync();

        return true;
    }

    private async Task AyniAdKontrolEtAsync(
        int kurumId,
        string ad,
        int? haricTutulanId = null)
    {
        var karsilastirilacakAd = AdiNormallestir(ad);

        // Aynı isim kontrolü yalnızca seçilen kurum içinde yapılır.
        // Pasif şubeler de kontrole dahildir.
        var sorgu = _context.Subeler
            .AsNoTracking()
            .Where(s => s.KurumId == kurumId);

        if (haricTutulanId.HasValue)
        {
            sorgu = sorgu.Where(
                s => s.Id != haricTutulanId.Value);
        }

        var mevcutAdlar = await sorgu
            .Select(s => s.Ad)
            .ToListAsync();

        var mevcut = mevcutAdlar.Any(mevcutAd =>
            string.Equals(
                AdiNormallestir(mevcutAd),
                karsilastirilacakAd,
                StringComparison.Ordinal));

        if (mevcut)
        {
            throw new ArgumentException(
                "Bu kurumda aynı isimde bir şube zaten mevcut.");
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