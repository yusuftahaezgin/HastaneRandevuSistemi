using System.ComponentModel.DataAnnotations;
using System.Data;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Services;

public class DoktorCalismaSaatiService : IDoktorCalismaSaatiService
{
    private readonly AppDbContext _context;

    // Çalışma programı işlemlerine ait kilit grubu.
    private const int CalismaProgramiKilitGrubu = 74125;

    public DoktorCalismaSaatiService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DoktorCalismaSaati>> ListeleAsync(
        int? doktorId = null)
    {
        var sorgu = _context.DoktorCalismaSaatleri
            .AsNoTracking()
            .Include(c => c.Doktor)
                .ThenInclude(d => d.Poliklinik)
                    .ThenInclude(p => p.Sube)
                        .ThenInclude(s => s!.Kurum)
            .AsQueryable();

        if (doktorId.HasValue)
        {
            sorgu = sorgu.Where(c => c.DoktorId == doktorId.Value);
        }

        return await sorgu
            .OrderBy(c => c.Doktor.Ad)
            .ThenBy(c => c.Doktor.Soyad)
            .ThenBy(c => c.DoktorId)
            // Pazartesi ilk, pazar son sırada.
            .ThenBy(c =>
                c.Gun == DayOfWeek.Sunday ? 7 : (int)c.Gun)
            .ThenBy(c => c.BaslangicSaati)
            .ToListAsync();
    }

    public async Task<DoktorCalismaSaati?> GetirAsync(int id)
    {
        return await _context.DoktorCalismaSaatleri
            .AsNoTracking()
            .Include(c => c.Doktor)
                .ThenInclude(d => d.Poliklinik)
                    .ThenInclude(p => p.Sube)
                        .ThenInclude(s => s!.Kurum)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Doktor>> SecilebilirDoktorlariListeleAsync()
    {
        return await _context.Doktorlar
            .AsNoTracking()
            .Include(d => d.Poliklinik)
                .ThenInclude(p => p.Sube)
                    .ThenInclude(s => s!.Kurum)
            .Where(d =>
                d.AktifMi
                && d.Poliklinik.AktifMi
                && d.Poliklinik.Sube != null
                && d.Poliklinik.Sube.AktifMi
                && d.Poliklinik.Sube.Kurum.AktifMi)
            .OrderBy(d => d.Poliklinik.Sube!.Ad)
            .ThenBy(d => d.Poliklinik.Ad)
            .ThenBy(d => d.Ad)
            .ThenBy(d => d.Soyad)
            .ToListAsync();
    }

    public async Task EkleAsync(
        DoktorCalismaSaatiFormViewModel model)
    {
        ModeliDogrula(model);

        // Saat ve süre kuralları modelin constructor'ında doğrulanır.
        var kayit = new DoktorCalismaSaati(
            model.DoktorId!.Value,
            model.Gun!.Value,
            model.BaslangicSaati!.Value,
            model.BitisSaati!.Value,
            model.RandevuSuresiDakika!.Value,
            model.AktifMi);

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted);

        await DoktorPrograminiKilitleAsync(kayit.DoktorId);

        await DoktoruKontrolEtAsync(
            kayit.DoktorId,
            aktifProgram: kayit.AktifMi);

        if (kayit.AktifMi)
        {
            await CakismayiKontrolEtAsync(
                kayit.DoktorId,
                kayit.Gun,
                kayit.BaslangicSaati,
                kayit.BitisSaati);
        }

        _context.DoktorCalismaSaatleri.Add(kayit);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<bool> GuncelleAsync(
        DoktorCalismaSaatiFormViewModel model)
    {
        ModeliDogrula(model);

        // Kilidi formdan gelen doktor bilgisine göre değil,
        // veritabanındaki gerçek doktor bilgisine göre alıyoruz.
        var doktorId = await KaydinDoktorunuGetirAsync(model.Id);

        if (!doktorId.HasValue)
        {
            return false;
        }

        if (model.DoktorId != doktorId.Value)
        {
            throw new ArgumentException(
                "Çalışma programının bağlı olduğu doktor değiştirilemez.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted);

        await DoktorPrograminiKilitleAsync(doktorId.Value);

        // Kilidi aldıktan sonra kaydın güncel halini okuyoruz.
        var kayit = await _context.DoktorCalismaSaatleri
            .FirstOrDefaultAsync(c => c.Id == model.Id);

        if (kayit is null)
        {
            return false;
        }

        await DoktoruKontrolEtAsync(
            kayit.DoktorId,
            aktifProgram: model.AktifMi);

        kayit.BilgileriGuncelle(
            model.Gun!.Value,
            model.BaslangicSaati!.Value,
            model.BitisSaati!.Value,
            model.RandevuSuresiDakika!.Value);

        if (model.AktifMi)
        {
            await CakismayiKontrolEtAsync(
                kayit.DoktorId,
                kayit.Gun,
                kayit.BaslangicSaati,
                kayit.BitisSaati,
                haricTutulanId: kayit.Id);
        }

        kayit.AktiflikAyarla(model.AktifMi);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }

    public async Task<bool> DurumDegistirAsync(int id)
    {
        var doktorId = await KaydinDoktorunuGetirAsync(id);

        if (!doktorId.HasValue)
        {
            return false;
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted);

        await DoktorPrograminiKilitleAsync(doktorId.Value);

        var kayit = await _context.DoktorCalismaSaatleri
            .FirstOrDefaultAsync(c => c.Id == id);

        if (kayit is null)
        {
            return false;
        }

        var aktifYap = !kayit.AktifMi;

        // Pasifleştirme serbesttir.
        // Aktifleştirme sırasında uygunluk ve çakışma tekrar kontrol edilir.
        if (aktifYap)
        {
            await DoktoruKontrolEtAsync(
                kayit.DoktorId,
                aktifProgram: true);

            await CakismayiKontrolEtAsync(
                kayit.DoktorId,
                kayit.Gun,
                kayit.BaslangicSaati,
                kayit.BitisSaati,
                haricTutulanId: kayit.Id);
        }

        kayit.AktiflikAyarla(aktifYap);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }

    private async Task<int?> KaydinDoktorunuGetirAsync(int id)
    {
        return await _context.DoktorCalismaSaatleri
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => (int?)c.DoktorId)
            .FirstOrDefaultAsync();
    }

    private async Task DoktoruKontrolEtAsync(
        int doktorId,
        bool aktifProgram)
    {
        // Doktorlar üzerinden sorgulayarak sağlık personeli
        // kimliğiyle çalışma programı oluşturulmasını da engelleriz.
        var doktor = await _context.Doktorlar
            .AsNoTracking()
            .Include(d => d.Poliklinik)
                .ThenInclude(p => p.Sube)
                    .ThenInclude(s => s!.Kurum)
            .FirstOrDefaultAsync(d => d.Id == doktorId);

        if (doktor is null)
        {
            throw new ArgumentException(
                "Seçilen doktor bulunamadı.");
        }

        // Mevcut pasif programlar, doktor veya üst birimleri
        // pasif olsa da düzenlenebilir.
        if (!aktifProgram)
        {
            return;
        }

        if (!doktor.AktifMi)
        {
            throw new ArgumentException(
                "Pasif doktora aktif çalışma programı tanımlanamaz.");
        }

        var poliklinik = doktor.Poliklinik;

        if (poliklinik is null || !poliklinik.AktifMi)
        {
            throw new ArgumentException(
                "Doktorun polikliniği bulunamadı veya pasif durumda. " +
                "Aktif çalışma programı tanımlanamaz.");
        }

        var sube = poliklinik.Sube;

        if (sube is null || !sube.AktifMi)
        {
            throw new ArgumentException(
                "Doktorun hastanesi bulunamadı veya pasif durumda. " +
                "Aktif çalışma programı tanımlanamaz.");
        }

        if (sube.Kurum is null || !sube.Kurum.AktifMi)
        {
            throw new ArgumentException(
                "Doktorun bağlı olduğu kurum bulunamadı veya pasif durumda. " +
                "Aktif çalışma programı tanımlanamaz.");
        }
    }

    private async Task CakismayiKontrolEtAsync(
        int doktorId,
        DayOfWeek gun,
        TimeOnly baslangicSaati,
        TimeOnly bitisSaati,
        int? haricTutulanId = null)
    {
        var sorgu = _context.DoktorCalismaSaatleri
            .AsNoTracking()
            .Where(c =>
                c.DoktorId == doktorId
                && c.Gun == gun
                && c.AktifMi);

        if (haricTutulanId.HasValue)
        {
            sorgu = sorgu.Where(c => c.Id != haricTutulanId.Value);
        }

        var cakismaVar = await sorgu.AnyAsync(c =>
            baslangicSaati < c.BitisSaati
            && bitisSaati > c.BaslangicSaati);

        if (cakismaVar)
        {
            throw new ArgumentException(
                "Bu doktorun seçilen günde, belirtilen saatlerle " +
                "çakışan aktif bir çalışma programı zaten var.");
        }
    }

    private async Task DoktorPrograminiKilitleAsync(int doktorId)
    {
        // Aynı doktor için bu servisten gelen yazma işlemleri
        // transaction bitene kadar sırayla yürütülür.
        // Değerler SQL parametresi olarak gönderilir.
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({CalismaProgramiKilitGrubu}, {doktorId})");
    }

    private static void ModeliDogrula(
        DoktorCalismaSaatiFormViewModel model)
    {
        var sonuclar = new List<ValidationResult>();

        var gecerli = Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            sonuclar,
            validateAllProperties: true);

        if (!gecerli)
        {
            throw new ArgumentException(
                string.Join(
                    " ",
                    sonuclar.Select(s =>
                        s.ErrorMessage ?? "Girilen bilgiler geçersiz.")));
        }
    }
}