using HastaneRandevuSistemi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Kurum> Kurumlar => Set<Kurum>();

    public DbSet<Sube> Subeler => Set<Sube>();

    public DbSet<Poliklinik> Poliklinikler => Set<Poliklinik>();

    public DbSet<Calisan> Calisanlar => Set<Calisan>();

    public DbSet<Doktor> Doktorlar => Set<Doktor>();

    public DbSet<SaglikPersoneli> SaglikPersonelleri
        => Set<SaglikPersoneli>();

    public DbSet<DoktorCalismaSaati> DoktorCalismaSaatleri
        => Set<DoktorCalismaSaati>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<HastaneCalismaSaati>(entity =>
        {
            entity.ToTable("HastaneCalismaSaatleri", table =>
            {
                table.HasCheckConstraint(
                    "CK_HastaneCalismaSaatleri_Gun",
                    "\"Gun\" BETWEEN 0 AND 6");

                table.HasCheckConstraint(
                    "CK_HastaneCalismaSaatleri_Saat",
                    "\"BaslangicDakika\" BETWEEN 0 AND 1439 " +
                    "AND \"BitisDakika\" BETWEEN 1 AND 1440 " +
                    "AND \"BitisDakika\" > \"BaslangicDakika\"");
            });

            entity.HasOne(x => x.Sube)
                .WithMany(x => x.CalismaSaatleri)
                .HasForeignKey(x => x.SubeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- KULLANICI ROLLERİ ----------------

        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "11111111-1111-1111-1111-111111111111",
                Name = "Hasta",
                NormalizedName = "HASTA",
                ConcurrencyStamp = "11111111-1111-1111-1111-111111111111"
            },
            new IdentityRole
            {
                Id = "22222222-2222-2222-2222-222222222222",
                Name = "Doktor",
                NormalizedName = "DOKTOR",
                ConcurrencyStamp = "22222222-2222-2222-2222-222222222222"
            },
            new IdentityRole
            {
                Id = "33333333-3333-3333-3333-333333333333",
                Name = "Yonetici",
                NormalizedName = "YONETICI",
                ConcurrencyStamp = "33333333-3333-3333-3333-333333333333"
            },
            new IdentityRole
            {
                Id = "44444444-4444-4444-4444-444444444444",
                Name = "SaglikPersoneli",
                NormalizedName = "SAGLIKPERSONELI",
                ConcurrencyStamp = "44444444-4444-4444-4444-444444444444"
            }
        );

        // ---------------- KURUMLAR ----------------

        var kurum = modelBuilder.Entity<Kurum>();

        kurum.ToTable("Kurumlar");

        kurum.HasKey(k => k.Id);

        kurum.Property(k => k.Ad)
            .IsRequired()
            .HasMaxLength(150);

        kurum.Property(k => k.Aciklama)
            .IsRequired()
            .HasMaxLength(300);

        kurum.Property(k => k.AktifMi)
            .IsRequired();

        // ---------------- ŞUBELER ----------------

        var sube = modelBuilder.Entity<Sube>();

        sube.ToTable("Subeler");

        sube.HasKey(s => s.Id);

        sube.Property(s => s.Ad)
            .IsRequired()
            .HasMaxLength(150);

        sube.Property(s => s.Adres)
            .IsRequired()
            .HasMaxLength(400);

        sube.Property(s => s.Telefon)
            .IsRequired()
            .HasMaxLength(30);

        sube.Property(s => s.AktifMi)
            .IsRequired();

        sube.HasOne(s => s.Kurum)
            .WithMany(k => k.Subeler)
            .HasForeignKey(s => s.KurumId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // ---------------- POLİKLİNİKLER ----------------

        var poliklinik = modelBuilder.Entity<Poliklinik>();

        poliklinik.ToTable("Poliklinikler");

        poliklinik.HasKey(p => p.Id);

        poliklinik.Property(p => p.Ad)
            .IsRequired()
            .HasMaxLength(120);

        poliklinik.Property(p => p.Aciklama)
            .IsRequired()
            .HasMaxLength(300);

        poliklinik.HasOne(p => p.Sube)
            .WithMany()
            .HasForeignKey(p => p.SubeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        poliklinik.HasData(
            new
            {
                Id = 1,
                Ad = "Dahiliye",
                Aciklama = "İç hastalıkları",
                AktifMi = true,
                SubeId = 2
            },
            new
            {
                Id = 2,
                Ad = "Kardiyoloji",
                Aciklama = "Kalp ve dolaşım sistemi hastalıkları.",
                AktifMi = true,
                SubeId = 2
            },
            new
            {
                Id = 3,
                Ad = "Çocuk Hastalıkları",
                Aciklama = "Çocuk sağlığı ve hastalıklarının değerlendirilmesi.",
                AktifMi = true,
                SubeId = 2
            },
            new
            {
                Id = 4,
                Ad = "Dermatoloji",
                Aciklama = "Cilt hastalıkları",
                AktifMi = true,
                SubeId = 2
            },
            new
            {
                Id = 5,
                Ad = "Ortopedi ve Travmatoloji",
                Aciklama = "Kemik, eklem ve kas-iskelet sistemi sağlığı.",
                AktifMi = true,
                SubeId = 2
            },
            new
            {
                Id = 6,
                Ad = "Göz Hastalıkları",
                Aciklama = "Göz ve görme sağlığı.",
                AktifMi = true,
                SubeId = 2
            },
            new
            {
                Id = 7,
                Ad = "Nöroloji",
                Aciklama = "Sinir sistemi hastalıkları",
                AktifMi = true,
                SubeId = 2
            },
            new
            {
                Id = 8,
                Ad = "Kulak Burun Boğaz",
                Aciklama = "Kulak, burun ve boğaz hastalıkları.",
                AktifMi = true,
                SubeId = 2
            },
            new
            {
                Id = 9,
                Ad = "Kadın Hastalıkları ve Doğum",
                Aciklama = "Kadın sağlığı ve gebelik takibi.",
                AktifMi = true,
                SubeId = 2
            },
            new
            {
                Id = 10,
                Ad = "Üroloji",
                Aciklama = "İdrar yolları ve ilgili hastalıklar.",
                AktifMi = true,
                SubeId = 2
            },
            new
            {
                Id = 11,
                Ad = "Genel Cerrahi",
                Aciklama = "Cerrahi değerlendirme ve tedavi süreçleri.",
                AktifMi = true,
                SubeId = 2
            },
            new
            {
                Id = 12,
                Ad = "Psikiyatri",
                Aciklama = "Ruh sağlığı değerlendirmeleri.",
                AktifMi = true,
                SubeId = 2
            }
        );

        // ---------------- ORTAK ÇALIŞAN TABLOSU ----------------

        var calisan = modelBuilder.Entity<Calisan>();

        calisan.ToTable("Calisanlar");

        calisan.HasKey(c => c.Id);

        calisan.Property(c => c.Ad)
            .IsRequired()
            .HasMaxLength(80);

        calisan.Property(c => c.Soyad)
            .IsRequired()
            .HasMaxLength(80);

        calisan.HasOne(c => c.Poliklinik)
            .WithMany(p => p.Calisanlar)
            .HasForeignKey(c => c.PoliklinikId)
            .OnDelete(DeleteBehavior.Restrict);

        calisan.HasOne(c => c.Kullanici)
            .WithOne()
            .HasForeignKey<Calisan>(c => c.KullaniciId)
            .OnDelete(DeleteBehavior.SetNull);

        calisan.HasDiscriminator<string>("CalisanTuru")
            .HasValue<Doktor>("Doktor")
            .HasValue<SaglikPersoneli>("SaglikPersoneli");

        calisan.Property<string>("CalisanTuru")
            .HasMaxLength(32)
            .HasDefaultValue("Doktor");

        // ---------------- DOKTOR ----------------

        var doktor = modelBuilder.Entity<Doktor>();

        doktor.Property(d => d.Unvan)
            .IsRequired()
            .HasMaxLength(80);

        // ---------------- SAĞLIK PERSONELİ ----------------

        var saglikPersoneli = modelBuilder.Entity<SaglikPersoneli>();

        saglikPersoneli.Property(s => s.Gorev)
            .IsRequired()
            .HasMaxLength(80);

        // ---------------- BAŞLANGIÇ DOKTOR VERİLERİ ----------------

        doktor.HasData(
            new Doktor
            {
                Id = 1,
                Unvan = "Uzm. Dr.",
                Ad = "Deniz",
                Soyad = "Kaya",
                PoliklinikId = 1,
                AktifMi = true
            },
            new Doktor
            {
                Id = 2,
                Unvan = "Uzm. Dr.",
                Ad = "Ece",
                Soyad = "Yılmaz",
                PoliklinikId = 2,
                AktifMi = true
            },
            new Doktor
            {
                Id = 3,
                Unvan = "Uzm. Dr.",
                Ad = "Mert",
                Soyad = "Demir",
                PoliklinikId = 3,
                AktifMi = true
            },
            new Doktor
            {
                Id = 4,
                Unvan = "Uzm. Dr.",
                Ad = "Selin",
                Soyad = "Arslan",
                PoliklinikId = 4,
                AktifMi = true
            },
            new Doktor
            {
                Id = 5,
                Unvan = "Uzm. Dr.",
                Ad = "Can",
                Soyad = "Yıldız",
                PoliklinikId = 5,
                AktifMi = true
            },
            new Doktor
            {
                Id = 6,
                Unvan = "Uzm. Dr.",
                Ad = "Elif",
                Soyad = "Aydın",
                PoliklinikId = 6,
                AktifMi = true
            },
            new Doktor
            {
                Id = 7,
                Unvan = "Uzm. Dr.",
                Ad = "Burak",
                Soyad = "Şahin",
                PoliklinikId = 7,
                AktifMi = true
            },
            new Doktor
            {
                Id = 8,
                Unvan = "Uzm. Dr.",
                Ad = "Derya",
                Soyad = "Koç",
                PoliklinikId = 8,
                AktifMi = true
            },
            new Doktor
            {
                Id = 9,
                Unvan = "Uzm. Dr.",
                Ad = "Zeynep",
                Soyad = "Çelik",
                PoliklinikId = 9,
                AktifMi = true
            },
            new Doktor
            {
                Id = 10,
                Unvan = "Uzm. Dr.",
                Ad = "Emre",
                Soyad = "Kurt",
                PoliklinikId = 10,
                AktifMi = true
            },
            new Doktor
            {
                Id = 11,
                Unvan = "Uzm. Dr.",
                Ad = "Ceren",
                Soyad = "Özkan",
                PoliklinikId = 11,
                AktifMi = true
            },
            new Doktor
            {
                Id = 12,
                Unvan = "Uzm. Dr.",
                Ad = "Kerem",
                Soyad = "Aslan",
                PoliklinikId = 12,
                AktifMi = true
            }
        );

        // ---------------- DOKTOR ÇALIŞMA SAATLERİ ----------------

        var calismaSaati = modelBuilder.Entity<DoktorCalismaSaati>();

        calismaSaati.ToTable("DoktorCalismaSaatleri", tablo =>
        {
            tablo.HasCheckConstraint(
                "CK_DoktorCalismaSaatleri_Gun",
                "\"Gun\" BETWEEN 0 AND 6");

            tablo.HasCheckConstraint(
                "CK_DoktorCalismaSaatleri_SaatAraligi",
                "\"BitisSaati\" > \"BaslangicSaati\"");

            tablo.HasCheckConstraint(
                "CK_DoktorCalismaSaatleri_RandevuSuresi",
                "\"RandevuSuresiDakika\" BETWEEN 5 AND 120");

            tablo.HasCheckConstraint(
                "CK_DoktorCalismaSaatleri_MinimumAralik",
                "EXTRACT(EPOCH FROM " +
                "(\"BitisSaati\" - \"BaslangicSaati\")) " +
                ">= \"RandevuSuresiDakika\" * 60");

            tablo.HasCheckConstraint(
                "CK_DoktorCalismaSaatleri_DakikaHassasiyeti",
                "EXTRACT(SECOND FROM \"BaslangicSaati\") = 0 " +
                "AND EXTRACT(SECOND FROM \"BitisSaati\") = 0");
        });

        calismaSaati.HasKey(c => c.Id);

        calismaSaati.Property(c => c.Gun)
            .HasConversion<int>()
            .IsRequired();

        calismaSaati.Property(c => c.BaslangicSaati)
            .HasColumnType("time without time zone")
            .IsRequired();

        calismaSaati.Property(c => c.BitisSaati)
            .HasColumnType("time without time zone")
            .IsRequired();

        calismaSaati.Property(c => c.RandevuSuresiDakika)
            .IsRequired();

        calismaSaati.Property(c => c.AktifMi)
            .IsRequired();

        calismaSaati.HasOne(c => c.Doktor)
            .WithMany()
            .HasForeignKey(c => c.DoktorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        calismaSaati.HasIndex(c => new
        {
            c.DoktorId,
            c.Gun
        });
    }
}