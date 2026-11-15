using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Models;

public static class UcretKurali
{
    public static void Dogrula(decimal ucret)
    {
        if (ucret < 0
            || ucret > 9999999999.99m
            || decimal.Round(ucret, 2) != ucret)
        {
            throw new ArgumentException(
                "Ücret sıfır veya pozitif ve en fazla iki ondalık basamaklı olmalıdır.");
        }
    }
}

[Table("PersonelHizmetleri")]
public class PersonelHizmeti
{
    public int Id { get; private set; }

    public int PersonelId { get; private set; }

    public SaglikPersoneli Personel { get; private set; } = null!;

    [MaxLength(150)]
    public string Ad { get; private set; } = "";

    public int SureDakika { get; private set; }

    [Precision(12, 2)]
    public decimal Ucret { get; private set; }

    public bool AktifMi { get; private set; }

    private PersonelHizmeti()
    {
    }

    public PersonelHizmeti(
        int personelId,
        string ad,
        int sure,
        decimal ucret,
        bool aktif)
    {
        if (personelId <= 0)
            throw new ArgumentException("Personel seçin.");

        PersonelId = personelId;
        Guncelle(ad, sure, ucret, aktif);
    }

    public void Guncelle(
        string ad,
        int sure,
        decimal ucret,
        bool aktif)
    {
        ad = ad?.Trim() ?? "";

        if (ad.Length is < 1 or > 150)
            throw new ArgumentException(
                "Hizmet adı 1–150 karakter olmalıdır.");

        if (sure is < 5 or > 120)
            throw new ArgumentException(
                "Hizmet süresi 5–120 dakika olmalıdır.");

        UcretKurali.Dogrula(ucret);

        Ad = ad;
        SureDakika = sure;
        Ucret = ucret;
        AktifMi = aktif;
    }
}

[Table("PersonelCalismaSaatleri")]
public class PersonelCalismaSaati
{
    public int Id { get; private set; }

    public int PersonelId { get; private set; }

    public SaglikPersoneli Personel { get; private set; } = null!;

    public DayOfWeek Gun { get; private set; }

    public TimeOnly Baslangic { get; private set; }

    public TimeOnly Bitis { get; private set; }

    private PersonelCalismaSaati()
    {
    }

    public PersonelCalismaSaati(
        int personelId,
        DayOfWeek gun,
        TimeOnly baslangic,
        TimeOnly bitis)
    {
        if (personelId <= 0 || !Enum.IsDefined(gun))
            throw new ArgumentException("Personel ve gün seçin.");

        if (bitis <= baslangic
            || baslangic.Ticks % TimeSpan.TicksPerMinute != 0
            || bitis.Ticks % TimeSpan.TicksPerMinute != 0)
        {
            throw new ArgumentException(
                "Saatler dakika hassasiyetinde olmalı; " +
                "bitiş başlangıçtan sonra olmalıdır.");
        }

        PersonelId = personelId;
        Gun = gun;
        Baslangic = baslangic;
        Bitis = bitis;
    }
}

public enum RandevuDurumu
{
    OnayBekliyor,
    Onaylandi,
    Reddedildi,
    IptalEdildi
}

[Table("Randevular")]
[Index(nameof(CalisanId), nameof(Tarih))]
[Index(nameof(HastaId), nameof(Tarih))]
public class Randevu
{
    public int Id { get; private set; }

    [MaxLength(450)]
    public string HastaId { get; private set; } = "";

    public ApplicationUser Hasta { get; private set; } = null!;

    public int CalisanId { get; private set; }

    public Calisan Calisan { get; private set; } = null!;

    public int? PersonelHizmetiId { get; private set; }

    public PersonelHizmeti? PersonelHizmeti { get; private set; }

    public int PoliklinikId { get; private set; }

    public DateOnly Tarih { get; private set; }

    public TimeOnly Baslangic { get; private set; }

    public TimeOnly Bitis { get; private set; }

    public int SureDakika { get; private set; }

    [Precision(12, 2)]
    public decimal Ucret { get; private set; }

    [MaxLength(150)]
    public string IslemAdi { get; private set; } = "";

    [MaxLength(250)]
    public string CalisanAdi { get; private set; } = "";

    [MaxLength(150)]
    public string HastaneAdi { get; private set; } = "";

    [MaxLength(120)]
    public string PoliklinikAdi { get; private set; } = "";

    public RandevuDurumu Durum { get; private set; }

    public DateTime OlusturmaZamaniUtc { get; private set; }

    private Randevu()
    {
    }

    public Randevu(
        string hastaId,
        Calisan calisan,
        DateOnly tarih,
        TimeOnly baslangic,
        int sure,
        decimal ucret,
        string islemAdi,
        int? hizmetId = null)
    {
        if (string.IsNullOrWhiteSpace(hastaId))
            throw new ArgumentException("Hasta bulunamadı.");

        if (sure is < 5 or > 120)
            throw new ArgumentException("Randevu süresi geçersiz.");

        UcretKurali.Dogrula(ucret);

        var bitis = baslangic.ToTimeSpan()
                    + TimeSpan.FromMinutes(sure);

        if (bitis >= TimeSpan.FromDays(1))
            throw new ArgumentException(
                "Randevu aynı gün bitmelidir.");

        HastaId = hastaId;
        CalisanId = calisan.Id;
        PersonelHizmetiId = hizmetId;
        PoliklinikId = calisan.PoliklinikId;

        Tarih = tarih;
        Baslangic = baslangic;
        Bitis = TimeOnly.FromTimeSpan(bitis);

        // Kayıt anındaki bilgiler saklanır.
        SureDakika = sure;
        Ucret = ucret;
        IslemAdi = islemAdi;
        CalisanAdi = calisan.GorunenAdGetir();
        HastaneAdi = calisan.Poliklinik.Sube!.Ad;
        PoliklinikAdi = calisan.Poliklinik.Ad;

        Durum = RandevuDurumu.OnayBekliyor;
        OlusturmaZamaniUtc = DateTime.UtcNow;
    }

    public void KararVer(bool onay)
    {
        if (Durum != RandevuDurumu.OnayBekliyor)
            throw new ArgumentException(
                "Randevu onay beklemiyor.");

        Durum = onay
            ? RandevuDurumu.Onaylandi
            : RandevuDurumu.Reddedildi;
    }

    public void IptalEt()
    {
        if (Durum != RandevuDurumu.OnayBekliyor
            && Durum != RandevuDurumu.Onaylandi)
        {
            throw new ArgumentException(
                "Bu randevu zaten kapatılmış.");
        }

        Durum = RandevuDurumu.IptalEdildi;
    }
}

public class TarifeYonetimModel
{
    public List<Doktor> Doktorlar { get; set; } = new();

    public List<SaglikPersoneli> Personeller { get; set; } = new();

    public List<PersonelHizmeti> Hizmetler { get; set; } = new();

    public List<PersonelCalismaSaati> Saatler { get; set; } = new();
}

public class PersonelRandevuModel
{
    public List<PersonelHizmeti> Hizmetler { get; set; } = new();

    public PersonelHizmeti? SeciliHizmet { get; set; }

    public DateOnly Tarih { get; set; }

    public List<TimeOnly> Saatler { get; set; } = new();
}