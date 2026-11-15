using System.Data;
using System.Globalization;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Services;

public class RandevuKayitService
{
    private readonly AppDbContext db;

    public RandevuKayitService(AppDbContext context)
    {
        db = context;
    }

    public static DateTime Simdi =>
        TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"));

    public static string DurumMetni(RandevuDurumu durum) => durum switch
    {
        RandevuDurumu.OnayBekliyor => "Onay bekliyor",
        RandevuDurumu.Onaylandi => "Onaylandı",
        RandevuDurumu.Reddedildi => "Reddedildi",
        _ => "İptal edildi"
    };

    public static decimal UcretOku(string? metin)
    {
        if (!decimal.TryParse(
                metin?.Trim().Replace(',', '.'),
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var ucret))
        {
            throw new ArgumentException(
                "Ücreti 1000 veya 1000,50 biçiminde yazın.");
        }

        UcretKurali.Dogrula(ucret);
        return ucret;
    }

    private Task KilitAsync() =>
        db.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(74126, 1)");

    private IQueryable<Calisan> Calisanlar() =>
        db.Calisanlar
            .AsNoTracking()
            .Include(x => x.Poliklinik)
                .ThenInclude(x => x.Sube)
                    .ThenInclude(x => x!.Kurum);

    private static void AktifKontrol(Calisan calisan)
    {
        if (!calisan.AktifMi
            || !calisan.Poliklinik.AktifMi
            || calisan.Poliklinik.Sube is null
            || !calisan.Poliklinik.Sube.AktifMi
            || !calisan.Poliklinik.Sube.Kurum.AktifMi)
        {
            throw new ArgumentException(
                "Çalışan veya bağlı olduğu poliklinik, " +
                "hastane ya da kurum pasif.");
        }
    }

    private static void TarihKontrol(DateOnly tarih)
    {
        var bugun = DateOnly.FromDateTime(Simdi);

        if (tarih < bugun || tarih > bugun.AddDays(30))
        {
            throw new ArgumentException(
                "Bugün ile 30 gün sonrası arasında tarih seçin.");
        }
    }

    private IQueryable<Randevu> AcikRandevular() =>
        db.Set<Randevu>().Where(x =>
            x.Durum == RandevuDurumu.OnayBekliyor
            || x.Durum == RandevuDurumu.Onaylandi);

    public async Task<TarifeYonetimModel> TarifelerAsync()
    {
        var calisanlar = await Calisanlar()
            .OrderBy(x => x.Ad)
            .ToListAsync();

        return new TarifeYonetimModel
        {
            Doktorlar = calisanlar.OfType<Doktor>().ToList(),

            Personeller = calisanlar
                .OfType<SaglikPersoneli>()
                .ToList(),

            Hizmetler = await db.Set<PersonelHizmeti>()
                .AsNoTracking()
                .Include(x => x.Personel)
                    .ThenInclude(x => x.Poliklinik)
                        .ThenInclude(x => x.Sube)
                .OrderBy(x => x.Ad)
                .ToListAsync(),

            Saatler = await db.Set<PersonelCalismaSaati>()
                .AsNoTracking()
                .Include(x => x.Personel)
                .OrderBy(x => x.Personel.Ad)
                .ThenBy(x => x.Gun)
                .ThenBy(x => x.Baslangic)
                .ToListAsync()
        };
    }

    public async Task DoktorUcretiAsync(
        int id,
        decimal ucret,
        string? hizmetAdi = null)
    {
        using var tx = await db.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        await KilitAsync();

        var doktor = await db.Doktorlar
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ArgumentException("Doktor bulunamadı.");

        if (hizmetAdi is not null)
            doktor.HizmetAdiAyarla(hizmetAdi);

        doktor.UcretAyarla(ucret);

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task HizmetKaydetAsync(
        int id,
        int personelId,
        string ad,
        int sure,
        decimal ucret,
        bool aktif)
    {
        using var tx = await db.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        await KilitAsync();

        var personel = await Calisanlar()
            .OfType<SaglikPersoneli>()
            .SingleOrDefaultAsync(x => x.Id == personelId)
            ?? throw new ArgumentException(
                "Sağlık personeli bulunamadı.");

        if (aktif)
            AktifKontrol(personel);

        var adlar = await db.Set<PersonelHizmeti>()
            .Where(x => x.PersonelId == personelId && x.Id != id)
            .Select(x => x.Ad)
            .ToListAsync();

        if (adlar.Any(x => string.Equals(
                x,
                ad?.Trim(),
                StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException(
                "Bu personelde aynı isimde hizmet zaten var.");
        }

        if (id == 0)
        {
            db.Add(new PersonelHizmeti(
                personelId, ad, sure, ucret, aktif));
        }
        else
        {
            var hizmet = await db.Set<PersonelHizmeti>()
                .SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException("Hizmet bulunamadı.");

            if (hizmet.PersonelId != personelId)
            {
                throw new ArgumentException(
                    "Hizmetin personeli değiştirilemez.");
            }

            hizmet.Guncelle(ad, sure, ucret, aktif);
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task SaatEkleAsync(
        int personelId,
        int gun,
        TimeOnly baslangic,
        TimeOnly bitis)
    {
        var kayit = new PersonelCalismaSaati(
            personelId, (DayOfWeek)gun, baslangic, bitis);

        using var tx = await db.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        await KilitAsync();

        var personel = await Calisanlar()
            .OfType<SaglikPersoneli>()
            .SingleOrDefaultAsync(x => x.Id == personelId)
            ?? throw new ArgumentException(
                "Sağlık personeli bulunamadı.");

        AktifKontrol(personel);

        var cakisma = await db.Set<PersonelCalismaSaati>()
            .AnyAsync(x =>
                x.PersonelId == personelId
                && x.Gun == kayit.Gun
                && x.Baslangic < bitis
                && baslangic < x.Bitis);

        if (cakisma)
        {
            throw new ArgumentException(
                "Personelin çakışan çalışma saati var.");
        }

        db.Add(kayit);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task SaatSilAsync(int id)
    {
        using var tx = await db.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        await KilitAsync();

        var kayit = await db.Set<PersonelCalismaSaati>()
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ArgumentException(
                "Çalışma saati bulunamadı.");

        var simdi = Simdi;
        var bugun = DateOnly.FromDateTime(simdi);

        var randevular = await AcikRandevular()
            .Where(x =>
                x.CalisanId == kayit.PersonelId
                && x.Tarih >= bugun)
            .ToListAsync();

        if (randevular.Any(x =>
                x.Tarih.ToDateTime(x.Bitis) > simdi
                && x.Tarih.DayOfWeek == kayit.Gun
                && x.Baslangic < kayit.Bitis
                && kayit.Baslangic < x.Bitis))
        {
            throw new ArgumentException(
                "Bu çalışma aralığında mevcut veya gelecek randevu var. " +
                "Önce ilgili randevuyu iptal edin veya reddedin.");
        }

        db.Remove(kayit);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task DoktorSaatleriniFiltreleAsync(
        RandevuSecimViewModel model,
        string hastaId)
    {
        if (model.SeciliDoktorId is null
            || model.SeciliTarih is null)
        {
            return;
        }

        var subeId = model.SeciliDoktor?.Poliklinik.SubeId;

        if (subeId is null)
        {
            model.Saatler.Clear();
            return;
        }

        var hastaneProgrami = await new HastaneCalismaService(db)
            .GunAsync(
                subeId.Value,
                model.SeciliTarih.Value.DayOfWeek);

        if (hastaneProgrami is null || !hastaneProgrami.AcikMi)
        {
            model.Saatler.Clear();
            return;
        }

        var dolu = await AcikRandevular()
            .AsNoTracking()
            .Where(x =>
                x.Tarih == model.SeciliTarih
                && (x.CalisanId == model.SeciliDoktorId
                    || x.HastaId == hastaId))
            .ToListAsync();

        model.Saatler = model.Saatler
            .Where(s =>
                hastaneProgrami.Kapsar(
                    s.BaslangicSaati, s.BitisSaati)
                && !dolu.Any(x =>
                    x.Baslangic < s.BitisSaati
                    && s.BaslangicSaati < x.Bitis))
            .ToList();
    }

    public async Task<PersonelRandevuModel> PersonelSecimAsync(
        int? hizmetId,
        DateOnly tarih,
        string hastaId)
    {
        TarihKontrol(tarih);

        var hizmetler = await db.Set<PersonelHizmeti>()
            .AsNoTracking()
            .Include(x => x.Personel)
                .ThenInclude(x => x.Poliklinik)
                    .ThenInclude(x => x.Sube)
                        .ThenInclude(x => x!.Kurum)
            .Where(x =>
                x.AktifMi
                && x.Personel.AktifMi
                && x.Personel.Poliklinik.AktifMi
                && x.Personel.Poliklinik.Sube != null
                && x.Personel.Poliklinik.Sube.AktifMi
                && x.Personel.Poliklinik.Sube.Kurum.AktifMi)
            .OrderBy(x => x.Ad)
            .ToListAsync();

        var model = new PersonelRandevuModel
        {
            Hizmetler = hizmetler,
            Tarih = tarih
        };

        if (hizmetId is null || hizmetId == 0)
            return model;

        var hizmet = hizmetler
            .SingleOrDefault(x => x.Id == hizmetId)
            ?? throw new ArgumentException(
                "Hizmet bulunamadı veya pasif durumda.");

        model.SeciliHizmet = hizmet;

        var subeId = hizmet.Personel.Poliklinik.SubeId;

        if (subeId is null)
            return model;

        var hastaneProgrami = await new HastaneCalismaService(db)
            .GunAsync(subeId.Value, tarih.DayOfWeek);

        if (hastaneProgrami is null || !hastaneProgrami.AcikMi)
            return model;

        var programlar = await db.Set<PersonelCalismaSaati>()
            .AsNoTracking()
            .Where(x =>
                x.PersonelId == hizmet.PersonelId
                && x.Gun == tarih.DayOfWeek)
            .ToListAsync();

        var dolu = await AcikRandevular()
            .AsNoTracking()
            .Where(x =>
                x.Tarih == tarih
                && (x.CalisanId == hizmet.PersonelId
                    || x.HastaId == hastaId))
            .ToListAsync();

        var sure = TimeSpan.FromMinutes(hizmet.SureDakika);
        var simdi = Simdi;

        foreach (var program in programlar)
        {
            for (var saat = program.Baslangic.ToTimeSpan();
                 saat + sure <= program.Bitis.ToTimeSpan();
                 saat += sure)
            {
                var baslangic = TimeOnly.FromTimeSpan(saat);
                var bitis = TimeOnly.FromTimeSpan(saat + sure);

                if (tarih.ToDateTime(baslangic) <= simdi)
                    continue;

                if (!hastaneProgrami.Kapsar(baslangic, bitis))
                    continue;

                if (dolu.Any(x =>
                        x.Baslangic < bitis
                        && baslangic < x.Bitis))
                {
                    continue;
                }

                model.Saatler.Add(baslangic);
            }
        }

        model.Saatler = model.Saatler
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        return model;
    }

    public async Task DoktorKaydetAsync(
        string hastaId,
        int doktorId,
        DateOnly tarih,
        TimeOnly saat,
        decimal gosterilenUcret,
        int gosterilenSure)
    {
        using var tx = await db.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        await KilitAsync();

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(74125, {doktorId})");

        var doktor = await Calisanlar()
            .OfType<Doktor>()
            .SingleOrDefaultAsync(x => x.Id == doktorId)
            ?? throw new ArgumentException("Doktor bulunamadı.");

        AktifKontrol(doktor);

        if (!doktor.RandevuUcreti.HasValue)
        {
            throw new ArgumentException(
                "Doktorun randevu ücreti henüz tanımlanmamış.");
        }

        var model = await new RandevuSecimService(db)
            .HazirlaAsync(
                null,
                null,
                doktor.PoliklinikId,
                doktor.Id,
                tarih);

        await DoktorSaatleriniFiltreleAsync(model, hastaId);

        var secilen = model.Saatler
            .SingleOrDefault(x => x.BaslangicSaati == saat)
            ?? throw new ArgumentException(
                "Saat artık uygun değil. Saatleri yenileyin.");

        if (doktor.RandevuUcreti.Value != gosterilenUcret
            || secilen.RandevuSuresiDakika != gosterilenSure)
        {
            throw new ArgumentException(
                "Ücret veya süre değişti. Güncel bilgileri " +
                "kontrol ederek tekrar seçim yapın.");
        }

        db.Add(new Randevu(
            hastaId,
            doktor,
            tarih,
            saat,
            secilen.RandevuSuresiDakika,
            doktor.RandevuUcreti.Value,
            doktor.HizmetAdi));

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task PersonelKaydetAsync(
        string hastaId,
        int hizmetId,
        DateOnly tarih,
        TimeOnly saat,
        decimal gosterilenUcret,
        int gosterilenSure)
    {
        using var tx = await db.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        await KilitAsync();

        var model = await PersonelSecimAsync(
            hizmetId, tarih, hastaId);

        var hizmet = model.SeciliHizmet
            ?? throw new ArgumentException("Hizmet seçin.");

        if (!model.Saatler.Contains(saat))
        {
            throw new ArgumentException(
                "Saat artık uygun değil. Saatleri yenileyin.");
        }

        if (hizmet.Ucret != gosterilenUcret
            || hizmet.SureDakika != gosterilenSure)
        {
            throw new ArgumentException(
                "Ücret veya süre değişti. Güncel bilgileri " +
                "kontrol ederek tekrar seçim yapın.");
        }

        db.Add(new Randevu(
            hastaId,
            hizmet.Personel,
            tarih,
            saat,
            hizmet.SureDakika,
            hizmet.Ucret,
            hizmet.Ad,
            hizmet.Id));

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public Task<List<Randevu>> ListeAsync(string? hastaId = null)
    {
        return db.Set<Randevu>()
            .AsNoTracking()
            .Include(x => x.Hasta)
            .Where(x => hastaId == null || x.HastaId == hastaId)
            .OrderByDescending(x => x.Tarih)
            .ThenBy(x => x.Baslangic)
            .ToListAsync();
    }

    public async Task IptalAsync(int id, string hastaId)
    {
        using var tx = await db.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        await KilitAsync();

        var randevu = await db.Set<Randevu>()
            .SingleOrDefaultAsync(x =>
                x.Id == id && x.HastaId == hastaId)
            ?? throw new ArgumentException("Randevu bulunamadı.");

        if (randevu.Tarih.ToDateTime(randevu.Baslangic) <= Simdi)
        {
            throw new ArgumentException(
                "Başlamış veya geçmiş randevu iptal edilemez.");
        }

        randevu.IptalEt();

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task AdminKararAsync(int id, string karar)
    {
        using var tx = await db.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        await KilitAsync();

        var randevu = await db.Set<Randevu>()
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ArgumentException("Randevu bulunamadı.");

        if (randevu.Tarih.ToDateTime(randevu.Baslangic) <= Simdi)
        {
            throw new ArgumentException(
                "Başlamış veya geçmiş randevu değiştirilemez.");
        }

        if (karar == "iptal")
        {
            randevu.IptalEt();
        }
        else if (karar is "onay" or "ret")
        {
            if (karar == "onay")
            {
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock(74125, {randevu.CalisanId})");

                var calisan = await Calisanlar()
                    .SingleAsync(x => x.Id == randevu.CalisanId);

                AktifKontrol(calisan);

                if (calisan.PoliklinikId != randevu.PoliklinikId)
                {
                    throw new ArgumentException(
                        "Çalışanın polikliniği değişmiş. " +
                        "Bu randevu onaylanamaz.");
                }

                var subeId = calisan.Poliklinik.SubeId
                    ?? throw new ArgumentException(
                        "Çalışanın hastanesi bulunamadı.");

                var hastaneProgrami = await new HastaneCalismaService(db)
                    .GunAsync(subeId, randevu.Tarih.DayOfWeek);

                if (hastaneProgrami is null
                    || !hastaneProgrami.Kapsar(
                        randevu.Baslangic, randevu.Bitis))
                {
                    throw new ArgumentException(
                        "Randevu hastanenin güncel çalışma saatleri dışında.");
                }

                bool uygun;

                if (calisan is Doktor)
                {
                    uygun = await db.DoktorCalismaSaatleri
                        .AnyAsync(x =>
                            x.DoktorId == calisan.Id
                            && x.AktifMi
                            && x.Gun == randevu.Tarih.DayOfWeek
                            && x.BaslangicSaati <= randevu.Baslangic
                            && randevu.Bitis <= x.BitisSaati);
                }
                else
                {
                    uygun = await db.Set<PersonelCalismaSaati>()
                        .AnyAsync(x =>
                            x.PersonelId == calisan.Id
                            && x.Gun == randevu.Tarih.DayOfWeek
                            && x.Baslangic <= randevu.Baslangic
                            && randevu.Bitis <= x.Bitis);
                }

                if (!uygun)
                {
                    throw new ArgumentException(
                        "Randevu güncel çalışma programına uymuyor.");
                }
            }

            randevu.KararVer(karar == "onay");
        }
        else
        {
            throw new ArgumentException("Geçersiz karar.");
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}