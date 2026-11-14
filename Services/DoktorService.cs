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

        await PoliklinikAtamasiniKontrolEtAsync(
            model.PoliklinikId);

        var mevcutKullanici =
            await _userManager.FindByEmailAsync(email);

        if (mevcutKullanici is not null)
        {
            throw new ArgumentException(
                "Bu e-posta adresi zaten kayıtlı.");
        }

        // Hesap, rol ve doktor kaydı birlikte oluşturulur.
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

        // Mevcut atama veritabanındaki kayıttan alınır.
        await PoliklinikAtamasiniKontrolEtAsync(
            model.PoliklinikId,
            doktor.PoliklinikId);

        string ad = model.Ad.Trim();
        string soyad = model.Soyad.Trim();
        string unvan = model.Unvan.Trim();

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        if (doktor.Kullanici is not null)
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

    private async Task PoliklinikAtamasiniKontrolEtAsync(
        int poliklinikId,
        int? mevcutPoliklinikId = null)
    {
        if (poliklinikId <= 0)
        {
            throw new ArgumentException(
                "Lütfen bir poliklinik seçin.");
        }

        var poliklinik = await _context.Poliklinikler
            .AsNoTracking()
            .Include(p => p.Sube)
                .ThenInclude(s => s!.Kurum)
            .FirstOrDefaultAsync(p => p.Id == poliklinikId);

        if (poliklinik is null)
        {
            throw new ArgumentException(
                "Seçilen poliklinik bulunamadı.");
        }

        if (poliklinik.Sube is null ||
            poliklinik.Sube.Kurum is null)
        {
            throw new ArgumentException(
                "Seçilen polikliniğin hastane veya kurum bağlantısı bulunamadı.");
        }

        // Atama değişmiyorsa mevcut kayıt korunabilir.
        // Böylece pasif bir birimdeki doktorun adı gibi
        // diğer bilgilerinin düzenlenmesi engellenmez.
        if (mevcutPoliklinikId == poliklinikId)
        {
            return;
        }

        if (!poliklinik.Sube.Kurum.AktifMi)
        {
            throw new ArgumentException(
                "Seçilen polikliniğin bağlı olduğu kurum pasiftir. Bu kuruma doktor atanamaz.");
        }

        if (!poliklinik.Sube.AktifMi)
        {
            throw new ArgumentException(
                "Seçilen polikliniğin bağlı olduğu hastane pasiftir. Bu hastaneye doktor atanamaz.");
        }

        if (!poliklinik.AktifMi)
        {
            throw new ArgumentException(
                "Seçilen poliklinik pasiftir. Doktor atamak için aktif bir poliklinik seçin.");
        }
    }

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