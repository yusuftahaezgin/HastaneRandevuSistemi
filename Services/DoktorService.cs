using System.ComponentModel.DataAnnotations;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Services;

public class DoktorService : IDoktorService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DoktorService(
        AppDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<Doktor>> ListeleAsync()
    {
        return await _context.Doktorlar
            .AsNoTracking()
            .Include(d => d.Poliklinik)
            .Include(d => d.Kullanici)
            .OrderBy(d => d.Poliklinik.Ad)
            .ThenBy(d => d.Ad)
            .ThenBy(d => d.Soyad)
            .ToListAsync();
    }

    public async Task<Doktor?> GetirAsync(int id)
    {
        return await _context.Doktorlar
            .AsNoTracking()
            .Include(d => d.Poliklinik)
            .Include(d => d.Kullanici)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task EkleAsync(DoktorEkleViewModel model)
    {
        ModeliDogrula(model);

        string ad = model.Ad.Trim();
        string soyad = model.Soyad.Trim();
        string unvan = model.Unvan.Trim();
        string email = model.Email.Trim();

        bool aktifPoliklinikMevcut = await _context.Poliklinikler
            .AnyAsync(p =>
                p.Id == model.PoliklinikId && p.AktifMi);

        if (!aktifPoliklinikMevcut)
        {
            throw new ArgumentException(
                "Geçerli ve aktif bir poliklinik seçmelisiniz.");
        }

        var mevcutKullanici =
            await _userManager.FindByEmailAsync(email);

        if (mevcutKullanici != null)
        {
            throw new ArgumentException(
                "Bu e-posta adresi zaten kayıtlı.");
        }

        // Kullanıcı hesabı, rol ve doktor kaydı birlikte oluşturulur.
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var kullanici = new ApplicationUser
        {
            Ad = ad,
            Soyad = soyad,
            Email = email,
            UserName = email,
            EmailConfirmed = true
        };

        var kullaniciSonucu = await _userManager.CreateAsync(
            kullanici,
            model.Parola);

        if (!kullaniciSonucu.Succeeded)
        {
            await transaction.RollbackAsync();

            string hatalar = string.Join(
                " ",
                kullaniciSonucu.Errors.Select(e => e.Description));

            throw new ArgumentException(hatalar);
        }

        var rolSonucu = await _userManager.AddToRoleAsync(
            kullanici,
            "Doktor");

        if (!rolSonucu.Succeeded)
        {
            await transaction.RollbackAsync();

            string hatalar = string.Join(
                " ",
                rolSonucu.Errors.Select(e => e.Description));

            throw new ArgumentException(hatalar);
        }

        var doktor = new Doktor
        {
            Unvan = unvan,
            Ad = ad,
            Soyad = soyad,
            PoliklinikId = model.PoliklinikId,
            AktifMi = true,
            KullaniciId = kullanici.Id
        };

        _context.Doktorlar.Add(doktor);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<bool> GuncelleAsync(
        DoktorDuzenleViewModel model)
    {
        ModeliDogrula(model);

        var doktor = await _context.Doktorlar
            .Include(d => d.Kullanici)
            .FirstOrDefaultAsync(d => d.Id == model.Id);

        if (doktor is null)
        {
            return false;
        }

        bool poliklinikMevcut = await _context.Poliklinikler
            .AnyAsync(p => p.Id == model.PoliklinikId);

        if (!poliklinikMevcut)
        {
            throw new ArgumentException(
                "Seçilen poliklinik bulunamadı.");
        }

        string ad = model.Ad.Trim();
        string soyad = model.Soyad.Trim();
        string unvan = model.Unvan.Trim();

        // Kullanıcı ve doktor bilgileri birlikte güncellenir.
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        if (doktor.Kullanici != null)
        {
            doktor.Kullanici.Ad = ad;
            doktor.Kullanici.Soyad = soyad;

            var kullaniciSonucu =
                await _userManager.UpdateAsync(doktor.Kullanici);

            if (!kullaniciSonucu.Succeeded)
            {
                await transaction.RollbackAsync();

                string hatalar = string.Join(
                    " ",
                    kullaniciSonucu.Errors.Select(e => e.Description));

                throw new ArgumentException(hatalar);
            }
        }

        doktor.Unvan = unvan;
        doktor.Ad = ad;
        doktor.Soyad = soyad;
        doktor.PoliklinikId = model.PoliklinikId;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }

    public async Task<bool> DurumDegistirAsync(int id)
    {
        var doktor = await _context.Doktorlar
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doktor is null)
        {
            return false;
        }

        doktor.AktifMi = !doktor.AktifMi;

        await _context.SaveChangesAsync();

        return true;
    }

    // ViewModel üzerindeki Required, StringLength gibi kuralları kontrol eder.
    private static void ModeliDogrula(object model)
    {
        var sonuclar = new List<ValidationResult>();

        bool gecerli = Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            sonuclar,
            validateAllProperties: true);

        if (!gecerli)
        {
            string hatalar = string.Join(
                " ",
                sonuclar.Select(
                    s => s.ErrorMessage ?? "Girilen bilgiler geçersiz."));

            throw new ArgumentException(hatalar);
        }
    }
}