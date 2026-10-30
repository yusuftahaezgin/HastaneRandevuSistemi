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

    public DbSet<Poliklinik> Poliklinikler => Set<Poliklinik>();

    public DbSet<Doktor> Doktorlar => Set<Doktor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Kullanıcı rolleri
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
            }
        );

        // Poliklinik tablosu
        var poliklinik = modelBuilder.Entity<Poliklinik>();

        poliklinik.ToTable("Poliklinikler");

        poliklinik.HasKey(p => p.Id);

        poliklinik.Property(p => p.Ad)
            .IsRequired()
            .HasMaxLength(120);

        poliklinik.Property(p => p.Aciklama)
            .HasMaxLength(300);

        poliklinik.HasData(
            new Poliklinik
            {
                Id = 1,
                Ad = "Dahiliye",
                Aciklama = "İç hastalıkları",
                AktifMi = true
            },
            new Poliklinik
            {
                Id = 2,
                Ad = "Kardiyoloji",
                Aciklama = "Kalp ve dolaşım sistemi hastalıkları.",
                AktifMi = true
            },
            new Poliklinik
            {
                Id = 3,
                Ad = "Çocuk Hastalıkları",
                Aciklama = "Çocuk sağlığı ve hastalıklarının değerlendirilmesi.",
                AktifMi = true
            },
            new Poliklinik
            {
                Id = 4,
                Ad = "Dermatoloji",
                Aciklama = "Cilt hastalıkları",
                AktifMi = true
            },
            new Poliklinik
            {
                Id = 5,
                Ad = "Ortopedi ve Travmatoloji",
                Aciklama = "Kemik, eklem ve kas-iskelet sistemi sağlığı.",
                AktifMi = true
            },
            new Poliklinik
            {
                Id = 6,
                Ad = "Göz Hastalıkları",
                Aciklama = "Göz ve görme sağlığı.",
                AktifMi = true
            },
            new Poliklinik
            {
                Id = 7,
                Ad = "Nöroloji",
                Aciklama = "Sinir sistemi hastalıkları",
                AktifMi = true
            },
            new Poliklinik
            {
                Id = 8,
                Ad = "Kulak Burun Boğaz",
                Aciklama = "Kulak, burun ve boğaz hastalıkları.",
                AktifMi = true
            },
            new Poliklinik
            {
                Id = 9,
                Ad = "Kadın Hastalıkları ve Doğum",
                Aciklama = "Kadın sağlığı ve gebelik takibi.",
                AktifMi = true
            },
            new Poliklinik
            {
                Id = 10,
                Ad = "Üroloji",
                Aciklama = "İdrar yolları ve ilgili hastalıklar.",
                AktifMi = true
            },
            new Poliklinik
            {
                Id = 11,
                Ad = "Genel Cerrahi",
                Aciklama = "Cerrahi değerlendirme ve tedavi süreçleri.",
                AktifMi = true
            },
            new Poliklinik
            {
                Id = 12,
                Ad = "Psikiyatri",
                Aciklama = "Ruh sağlığı değerlendirmeleri.",
                AktifMi = true
            }
        );

        // Doktor tablosu
        var doktor = modelBuilder.Entity<Doktor>();

        doktor.ToTable("Doktorlar");

        doktor.HasKey(d => d.Id);

        doktor.Property(d => d.Unvan)
            .IsRequired()
            .HasMaxLength(80);

        doktor.Property(d => d.Ad)
            .IsRequired()
            .HasMaxLength(80);

        doktor.Property(d => d.Soyad)
            .IsRequired()
            .HasMaxLength(80);

        doktor.HasOne(d => d.Poliklinik)
            .WithMany(p => p.Doktorlar)
            .HasForeignKey(d => d.PoliklinikId)
            .OnDelete(DeleteBehavior.Restrict);

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
    }
}