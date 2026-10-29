using HastaneRandevuSistemi.Models;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Poliklinik> Poliklinikler => Set<Poliklinik>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
                Aciklama = "İç hastalıklarının değerlendirilmesi.",
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
                Aciklama = "Cilt, saç ve tırnak hastalıkları.",
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
                Aciklama = "Sinir sistemi hastalıklarının değerlendirilmesi.",
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
    }
}