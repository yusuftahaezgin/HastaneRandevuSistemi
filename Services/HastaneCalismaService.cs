using System.Data;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Services;

public class HastaneCalismaService
{
    private readonly AppDbContext db;

    public HastaneCalismaService(AppDbContext context)
    {
        db = context;
    }

    public Task<List<Sube>> ListeAsync()
    {
        return db.Subeler
            .AsNoTracking()
            .Include(x => x.Kurum)
            .Include(x => x.CalismaSaatleri)
            .OrderBy(x => x.Kurum.Ad)
            .ThenBy(x => x.Ad)
            .ToListAsync();
    }

    public Task<HastaneCalismaSaati?> GunAsync(
        int subeId,
        DayOfWeek gun)
    {
        return db.Set<HastaneCalismaSaati>()
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.SubeId == subeId && x.Gun == gun);
    }

    public async Task KaydetAsync(
        int subeId,
        int gun,
        bool acikMi,
        string baslangic,
        string bitis)
    {
        if (!Enum.IsDefined(typeof(DayOfWeek), gun))
            throw new ArgumentException("Geçerli bir gün seçin.");

        var baslangicDakika =
            HastaneCalismaSaati.SaatOku(baslangic, false);

        var bitisDakika =
            HastaneCalismaSaati.SaatOku(bitis, true);

        var yeniProgram = new HastaneCalismaSaati((DayOfWeek)gun);
        yeniProgram.Guncelle(acikMi, baslangicDakika, bitisDakika);

        using var tx = await db.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        // Randevu kaydı ve onayı ile aynı kilit.
        await db.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(74126, 1)");

        var mevcut = await db.Set<HastaneCalismaSaati>()
            .SingleOrDefaultAsync(x =>
                x.SubeId == subeId && x.Gun == (DayOfWeek)gun)
            ?? throw new ArgumentException(
                "Hastanenin çalışma günü kaydı bulunamadı.");

        var simdi = RandevuKayitService.Simdi;
        var bugun = DateOnly.FromDateTime(simdi);

        // Randevunun kaydedildiği poliklinik üzerinden hastaneyi bul.
        var randevular = await db.Set<Randevu>()
            .AsNoTracking()
            .Where(x =>
                x.Tarih >= bugun
                && (x.Durum == RandevuDurumu.OnayBekliyor
                    || x.Durum == RandevuDurumu.Onaylandi)
                && db.Poliklinikler.Any(p =>
                    p.Id == x.PoliklinikId && p.SubeId == subeId))
            .ToListAsync();

        var etkilenenRandevuVar = randevular.Any(x =>
            x.Tarih.DayOfWeek == (DayOfWeek)gun
            && x.Tarih.ToDateTime(x.Bitis) > simdi
            && !yeniProgram.Kapsar(x.Baslangic, x.Bitis));

        if (etkilenenRandevuVar)
        {
            throw new ArgumentException(
                "Bu değişiklik mevcut veya gelecek bir randevuyu " +
                "hastanenin çalışma saatleri dışında bırakıyor. " +
                "Önce ilgili randevuları düzenleyin.");
        }

        mevcut.Guncelle(acikMi, baslangicDakika, bitisDakika);

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}