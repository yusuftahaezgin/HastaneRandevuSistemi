using System.ComponentModel.DataAnnotations;
using System.Data;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Services;

public class DoktorCalismaSaatiService : IDoktorCalismaSaatiService
{
    private readonly AppDbContext db;

    public DoktorCalismaSaatiService(AppDbContext context)
    {
        db = context;
    }

    private IQueryable<DoktorCalismaSaati> Sorgu() =>
        db.DoktorCalismaSaatleri
            .AsNoTracking()
            .Include(x => x.Doktor)
                .ThenInclude(x => x.Poliklinik)
                    .ThenInclude(x => x.Sube)
                        .ThenInclude(x => x!.Kurum);

    public Task<List<DoktorCalismaSaati>> ListeleAsync(
        int? doktorId = null) =>
        Sorgu()
            .Where(x =>
                doktorId == null || x.DoktorId == doktorId)
            .OrderBy(x => x.Doktor.Ad)
            .ThenBy(x => x.Doktor.Soyad)
            .ThenBy(x => x.DoktorId)
            .ThenBy(x =>
                x.Gun == DayOfWeek.Sunday ? 7 : (int)x.Gun)
            .ThenBy(x => x.BaslangicSaati)
            .ToListAsync();

    public Task<DoktorCalismaSaati?> GetirAsync(int id) =>
        Sorgu().SingleOrDefaultAsync(x => x.Id == id);

    public Task<List<Doktor>> SecilebilirDoktorlariListeleAsync() =>
        db.Doktorlar
            .AsNoTracking()
            .Include(x => x.Poliklinik)
                .ThenInclude(x => x.Sube)
                    .ThenInclude(x => x!.Kurum)
            .Where(x =>
                x.AktifMi
                && x.Poliklinik.AktifMi
                && x.Poliklinik.Sube != null
                && x.Poliklinik.Sube.AktifMi
                && x.Poliklinik.Sube.Kurum.AktifMi)
            .OrderBy(x => x.Poliklinik.Sube!.Ad)
            .ThenBy(x => x.Poliklinik.Ad)
            .ThenBy(x => x.Ad)
            .ThenBy(x => x.Soyad)
            .ToListAsync();

    private Task KilitleAsync(int doktorId) =>
        db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(74125, {doktorId})");

    private static void Dogrula(
        DoktorCalismaSaatiFormViewModel model)
    {
        var hatalar = new List<ValidationResult>();

        if (!Validator.TryValidateObject(
                model,
                new ValidationContext(model),
                hatalar,
                true))
        {
            throw new ArgumentException(
                string.Join(
                    " ",
                    hatalar.Select(x => x.ErrorMessage)));
        }
    }

    private async Task DoktorKontrolAsync(int id, bool aktif)
    {
        var doktor = await db.Doktorlar
            .AsNoTracking()
            .Include(x => x.Poliklinik)
                .ThenInclude(x => x.Sube)
                    .ThenInclude(x => x!.Kurum)
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ArgumentException(
                "Doktor bulunamadı.");

        if (aktif
            && (!doktor.AktifMi
                || !doktor.Poliklinik.AktifMi
                || doktor.Poliklinik.Sube is null
                || !doktor.Poliklinik.Sube.AktifMi
                || !doktor.Poliklinik.Sube.Kurum.AktifMi))
        {
            throw new ArgumentException(
                "Aktif program için doktor, poliklinik, " +
                "hastane ve kurum aktif olmalıdır.");
        }
    }

    private async Task CakismaAsync(
        DoktorCalismaSaati kayit)
    {
        if (kayit.AktifMi
            && await db.DoktorCalismaSaatleri.AnyAsync(x =>
                x.Id != kayit.Id
                && x.DoktorId == kayit.DoktorId
                && x.Gun == kayit.Gun
                && x.AktifMi
                && x.BaslangicSaati < kayit.BitisSaati
                && kayit.BaslangicSaati < x.BitisSaati))
        {
            throw new ArgumentException(
                "Bu doktorun seçilen günde, belirtilen saatlerle " +
                "çakışan aktif bir çalışma programı zaten var.");
        }
    }

    private async Task RandevulariKoruAsync(
        DoktorCalismaSaati yeni)
    {
        var simdi = RandevuKayitService.Simdi;
        var bugun = DateOnly.FromDateTime(simdi);

        var programlar = await db.DoktorCalismaSaatleri
            .AsNoTracking()
            .Where(x =>
                x.DoktorId == yeni.DoktorId
                && x.Id != yeni.Id
                && x.AktifMi)
            .ToListAsync();

        if (yeni.AktifMi)
        {
            programlar.Add(yeni);
        }

        var randevular = await db.Set<Randevu>()
            .AsNoTracking()
            .Where(x =>
                x.CalisanId == yeni.DoktorId
                && x.Tarih >= bugun
                && (x.Durum == RandevuDurumu.OnayBekliyor
                    || x.Durum == RandevuDurumu.Onaylandi))
            .ToListAsync();

        // Henüz bitmemiş randevuları korur.
        // Böylece başlamış ancak devam eden randevular da
        // çalışma programının dışında bırakılamaz.
        bool disaridaKalanVar = randevular.Any(r =>
            r.Tarih.ToDateTime(r.Bitis) > simdi
            && !programlar.Any(p =>
                p.Gun == r.Tarih.DayOfWeek
                && p.BaslangicSaati <= r.Baslangic
                && r.Bitis <= p.BitisSaati));

        if (disaridaKalanVar)
        {
            throw new ArgumentException(
                "Bu değişiklik, onay bekleyen veya onaylanmış " +
                "ve henüz bitmemiş randevuları çalışma programı " +
                "dışında bırakıyor. Gelecek randevular için önce " +
                "uygun iptal veya ret işlemini yapın; devam eden " +
                "randevuların bitmesini bekleyin.");
        }
    }

    public async Task EkleAsync(
        DoktorCalismaSaatiFormViewModel model)
    {
        Dogrula(model);

        var kayit = new DoktorCalismaSaati(
            model.DoktorId!.Value,
            model.Gun!.Value,
            model.BaslangicSaati!.Value,
            model.BitisSaati!.Value,
            model.RandevuSuresiDakika!.Value,
            model.AktifMi);

        using var tx = await db.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        await KilitleAsync(kayit.DoktorId);
        await DoktorKontrolAsync(
            kayit.DoktorId,
            kayit.AktifMi);

        await CakismaAsync(kayit);

        db.Add(kayit);

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task<bool> GuncelleAsync(
        DoktorCalismaSaatiFormViewModel model)
    {
        Dogrula(model);

        var doktorId = await db.DoktorCalismaSaatleri
            .Where(x => x.Id == model.Id)
            .Select(x => (int?)x.DoktorId)
            .SingleOrDefaultAsync();

        if (doktorId is null)
        {
            return false;
        }

        if (doktorId != model.DoktorId)
        {
            throw new ArgumentException(
                "Çalışma programının doktoru değiştirilemez.");
        }

        using var tx = await db.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        await KilitleAsync(doktorId.Value);

        var kayit = await db.DoktorCalismaSaatleri
            .SingleOrDefaultAsync(x => x.Id == model.Id);

        if (kayit is null)
        {
            return false;
        }

        await DoktorKontrolAsync(
            kayit.DoktorId,
            model.AktifMi);

        kayit.BilgileriGuncelle(
            model.Gun!.Value,
            model.BaslangicSaati!.Value,
            model.BitisSaati!.Value,
            model.RandevuSuresiDakika!.Value);

        kayit.AktiflikAyarla(model.AktifMi);

        await CakismaAsync(kayit);
        await RandevulariKoruAsync(kayit);

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> DurumDegistirAsync(int id)
    {
        var doktorId = await db.DoktorCalismaSaatleri
            .Where(x => x.Id == id)
            .Select(x => (int?)x.DoktorId)
            .SingleOrDefaultAsync();

        if (doktorId is null)
        {
            return false;
        }

        using var tx = await db.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        await KilitleAsync(doktorId.Value);

        var kayit = await db.DoktorCalismaSaatleri
            .SingleOrDefaultAsync(x => x.Id == id);

        if (kayit is null)
        {
            return false;
        }

        kayit.DurumDegistir();

        await DoktorKontrolAsync(
            kayit.DoktorId,
            kayit.AktifMi);

        await CakismaAsync(kayit);
        await RandevulariKoruAsync(kayit);

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return true;
    }
}