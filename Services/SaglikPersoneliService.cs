using System.ComponentModel.DataAnnotations;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Services;

public class SaglikPersoneliService : ISaglikPersoneliService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public SaglikPersoneliService(
        AppDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<SaglikPersoneli>> ListeleAsync()
    {
        return await _context.Set<SaglikPersoneli>()
            .AsNoTracking()
            .Include(p => p.Poliklinik)
            .Include(p => p.Kullanici)
            .OrderBy(p => p.Poliklinik.Ad)
            .ThenBy(p => p.Ad)
            .ThenBy(p => p.Soyad)
            .ToListAsync();
    }

    public async Task<SaglikPersoneli?> GetirAsync(int id)
    {
        return await _context.Set<SaglikPersoneli>()
            .AsNoTracking()
            .Include(p => p.Poliklinik)
            .Include(p => p.Kullanici)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task EkleAsync(SaglikPersoneliEkleViewModel model)
    {
        model.Gorev = (model.Gorev ?? string.Empty).Trim();
        model.Ad = (model.Ad ?? string.Empty).Trim();
        model.Soyad = (model.Soyad ?? string.Empty).Trim();
        model.Email = (model.Email ?? string.Empty).Trim();

        ModeliDogrula(model);

        var aktifPoliklinikVar = await _context.Poliklinikler
            .AnyAsync(p => p.Id == model.PoliklinikId && p.AktifMi);

        if (!aktifPoliklinikVar)
        {
            throw new ArgumentException(
                "Personel eklemek için aktif bir poliklinik seçmelisiniz.");
        }

        var mevcutKullanici =
            await _userManager.FindByEmailAsync(model.Email);

        if (mevcutKullanici is not null)
        {
            throw new ArgumentException(
                "Bu e-posta adresiyle kayıtlı bir kullanıcı zaten mevcut.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var kullanici = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            Ad = model.Ad,
            Soyad = model.Soyad,
            EmailConfirmed = true
        };

        var hesapSonucu =
            await _userManager.CreateAsync(kullanici, model.Parola);

        if (!hesapSonucu.Succeeded)
        {
            throw new ArgumentException(
                string.Join(" ", hesapSonucu.Errors.Select(e => e.Description)));
        }

        var rolSonucu =
            await _userManager.AddToRoleAsync(kullanici, "SaglikPersoneli");

        if (!rolSonucu.Succeeded)
        {
            throw new ArgumentException(
                string.Join(" ", rolSonucu.Errors.Select(e => e.Description)));
        }

        var personel = new SaglikPersoneli
        {
            Gorev = model.Gorev,
            Ad = model.Ad,
            Soyad = model.Soyad,
            PoliklinikId = model.PoliklinikId,
            AktifMi = true,
            KullaniciId = kullanici.Id
        };

        _context.Set<SaglikPersoneli>().Add(personel);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<bool> GuncelleAsync(
        SaglikPersoneliDuzenleViewModel model)
    {
        model.Gorev = (model.Gorev ?? string.Empty).Trim();
        model.Ad = (model.Ad ?? string.Empty).Trim();
        model.Soyad = (model.Soyad ?? string.Empty).Trim();

        ModeliDogrula(model);

        var personel = await _context.Set<SaglikPersoneli>()
            .Include(p => p.Kullanici)
            .FirstOrDefaultAsync(p => p.Id == model.Id);

        if (personel is null)
        {
            return false;
        }

        var poliklinik = await _context.Poliklinikler
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == model.PoliklinikId);

        if (poliklinik is null)
        {
            throw new ArgumentException("Seçilen poliklinik bulunamadı.");
        }

        // Mevcut polikliniği sonradan pasif olmuşsa korunabilir.
        // Başka bir pasif polikliniğe personel atanamaz.
        if (!poliklinik.AktifMi &&
            personel.PoliklinikId != model.PoliklinikId)
        {
            throw new ArgumentException(
                "Personeli başka bir polikliniğe taşımak için aktif bir poliklinik seçmelisiniz.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        if (personel.Kullanici is not null)
        {
            personel.Kullanici.Ad = model.Ad;
            personel.Kullanici.Soyad = model.Soyad;

            var hesapSonucu =
                await _userManager.UpdateAsync(personel.Kullanici);

            if (!hesapSonucu.Succeeded)
            {
                throw new ArgumentException(
                    string.Join(" ",
                        hesapSonucu.Errors.Select(e => e.Description)));
            }
        }

        personel.Gorev = model.Gorev;
        personel.Ad = model.Ad;
        personel.Soyad = model.Soyad;
        personel.PoliklinikId = model.PoliklinikId;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }

    public async Task<bool> DurumDegistirAsync(int id)
    {
        var personel = await _context.Set<SaglikPersoneli>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (personel is null)
        {
            return false;
        }

        personel.AktifMi = !personel.AktifMi;

        await _context.SaveChangesAsync();

        return true;
    }

    private static void ModeliDogrula(object model)
    {
        var sonuclar = new List<ValidationResult>();
        var dogrulamaBaglami = new ValidationContext(model);

        var gecerli = Validator.TryValidateObject(
            model,
            dogrulamaBaglami,
            sonuclar,
            validateAllProperties: true);

        if (!gecerli)
        {
            throw new ArgumentException(
                string.Join(" ", sonuclar.Select(s => s.ErrorMessage)));
        }
    }
}