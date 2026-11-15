
# Hastane Randevu Sistemi

## Proje Hakkında

Bu proje, Sakarya Üniversitesi Bilgisayar Mühendisliği
Yüksek Lisans programında verilen Nesneye Dayalı
Programlama Dilleri dersi kapsamında geliştirilmiştir.

Projenin amacı, nesneye dayalı programlama (OOP)
prensiplerini kullanarak hastane ve kliniklerin
randevu süreçlerini yönetebilen web tabanlı
bir otomasyon sistemi oluşturmaktır.

Uygulama; hastaların doktor ve sağlık personelinden
randevu almasını, yöneticilerin sağlık kuruluşlarını
ve çalışanları yönetmesini, randevu taleplerinin
onaylanmasını ve çalışma saatlerine göre uygun
randevu zamanlarının belirlenmesini sağlamaktadır.

Sistem birden fazla kurum, hastane/şube ve
poliklinik yapısını desteklemektedir.

## Kullanılan Teknolojiler

- **Programlama Dili:** C#
- **Framework:** ASP.NET Core MVC (.NET 10)
- **Veritabanı:** PostgreSQL
- **ORM:** Entity Framework Core
- **Kimlik Doğrulama:** ASP.NET Core Identity
- **Arayüz:** HTML, CSS, Bootstrap, Razor Views
- **Geliştirme Ortamı:** Visual Studio Code
- **Versiyon Kontrol:** Git ve GitHub

## Proje Özellikleri

### 1. Hastane ve Klinik Yönetimi

- Sağlık kurumu ekleme ve düzenleme
- Kurumlara bağlı hastane/şube oluşturma
- Poliklinik tanımlama ve yönetme
- Kurum, şube ve poliklinik aktiflik yönetimi
- Hastane çalışma günlerini ve saatlerini düzenleme
- Birden fazla sağlık kuruluşunu destekleme

### 2. Doktor Yönetimi

- Doktor ekleme ve düzenleme
- Doktorları polikliniklere atama
- Doktor unvanı ve uzmanlık birimi yönetimi
- Doktorların aktif/pasif durumlarını değiştirme
- Doktor çalışma günlerini ve saatlerini tanımlama
- Muayene hizmeti ve ücretini düzenleme
- Doktor giriş hesaplarının oluşturulması
- Yönetici tarafından doktor parolasının sıfırlanması

### 3. Sağlık Personeli Yönetimi

- Sağlık personeli ekleme ve düzenleme
- Personelin görev ve poliklinik bilgilerinin yönetimi
- Sağlık personeline hizmet tanımlama
- Hizmetlerin süre ve ücret bilgilerinin belirlenmesi
- Personel çalışma saatlerinin yönetimi
- Personelin aktif/pasif durumunun değiştirilmesi

### 4. Randevu Yönetimi

- Doktor muayenesi için randevu oluşturma
- Sağlık personeli hizmetleri için randevu oluşturma
- Hastane, poliklinik ve çalışan seçimi
- Tarihe göre uygun randevu saatlerini listeleme
- Randevu çakışmalarını engelleme
- Hastane ve çalışan çalışma saatlerini kontrol etme
- Randevu süresi ve ücretinin kaydedilmesi
- Yönetici onay mekanizması
- Randevu reddetme ve iptal etme
- Kullanıcıların kendi randevularını görüntülemesi

## Kullanıcı Rolleri

Sistemde dört farklı kullanıcı rolü bulunmaktadır.

### Hasta

- Sisteme kayıt olabilir ve giriş yapabilir.
- Hastane, poliklinik ve doktor seçebilir.
- Uygun saatlerden randevu oluşturabilir.
- Sağlık personeli hizmetlerine randevu alabilir.
- Randevu durumlarını görüntüleyebilir.
- Randevularını iptal edebilir.

### Doktor

- Kendisine alınan randevuları görüntüleyebilir.
- Randevuların tarih, saat ve hasta bilgilerine erişebilir.
- Randevuların durumunu takip edebilir.

### Sağlık Personeli

- Kendisine oluşturulan hizmet randevularını görebilir.
- Hasta, işlem, süre ve ücret bilgilerini inceleyebilir.
- Randevu durumlarını takip edebilir.

### Yönetici

- Kurum, hastane ve poliklinikleri yönetebilir.
- Doktor ve sağlık personeli ekleyebilir.
- Çalışanların çalışma saatlerini düzenleyebilir.
- Sağlık hizmetlerini ve ücretlerini yönetebilir.
- Randevu taleplerini onaylayabilir veya reddedebilir.
- Randevuları iptal edebilir.
- Doktorların giriş parolalarını sıfırlayabilir.

## Nesneye Dayalı Programlama (OOP)

Projede nesneye dayalı programlamanın temel
prensipleri uygulanmıştır.

### Kalıtım (Inheritance)

`Calisan` soyut sınıfı, doktor ve sağlık personeli
için ortak özellikleri tanımlar.

`Doktor` ve `SaglikPersoneli` sınıfları,
`Calisan` sınıfından kalıtım alır.

Bu sayede çalışanların ortak özellikleri
tekrar yazılmadan kullanılabilir.

### Çok Biçimlilik (Polymorphism)

`Calisan` sınıfında tanımlanan
`GorunenAdGetir()` metodu, alt sınıflarda
farklı biçimlerde uygulanır.

Doktor sınıfında unvan bilgisi,
sağlık personeli sınıfında ise görev bilgisi
kullanılarak görünen ad oluşturulur.

### Kapsülleme (Encapsulation)

Sınıfların bazı özellikleri kontrollü
güncelleme metotları üzerinden yönetilir.

Örneğin `Doktor` sınıfında hizmet adı ve
randevu ücreti, doğrulama içeren metotlarla
güncellenmektedir.

Bu yaklaşım, nesnelerin geçersiz verilere
karşı korunmasına yardımcı olur.

### Soyutlama (Abstraction)

Projede `Calisan` gibi soyut sınıflar ve
servis arayüzleri kullanılmaktadır.

`IDoktorService`, `IPoliklinikService`,
`IKurumService` gibi arayüzler,
işlemlerin sözleşmelerini tanımlar.

Bu yapı, uygulamanın bakımını ve
geliştirilmesini kolaylaştırır.

## Proje Mimarisi

Uygulama ASP.NET Core MVC mimarisi
kullanılarak geliştirilmiştir.

Proje ayrıca servis katmanı aracılığıyla
iş kurallarını controller sınıflarından ayırır.

### Klasör Yapısı

```text
HastaneRandevuSistemi/
|
|-- Controllers/
|   |-- AdminController.cs
|   |-- AccountController.cs
|   |-- RandevuController.cs
|   |-- CalisanController.cs
|   ...
|
|-- Models/
|   |-- Calisan.cs
|   |-- Doktor.cs
|   |-- SaglikPersoneli.cs
|   |-- ApplicationUser.cs
|   ...
|
|-- Services/
|   |-- Interfaces/
|   |-- DoktorService.cs
|   |-- RandevuKayitService.cs
|   |-- RandevuSecimService.cs
|   ...
|
|-- Data/
|   |-- AppDbContext.cs
|
|-- Filters/
|
|-- Views/
|   |-- Admin/
|   |-- Account/
|   |-- Randevu/
|   |-- Calisan/
|   |-- Shared/
|   ...
|
|-- Migrations/
|
|-- wwwroot/
|
|-- Program.cs
|-- appsettings.json
|-- HastaneRandevuSistemi.csproj
|-- README.md
```

### Katmanların Görevleri

**Models:** Veritabanı varlıklarını ve
uygulamanın veri yapılarını tanımlar.

**Views:** Kullanıcıların gördüğü
Razor arayüzlerini içerir.

**Controllers:** Kullanıcı isteklerini karşılar
ve ilgili işlemleri yönlendirir.

**Services:** İş kurallarını,
randevu yönetimini ve doğrulamaları gerçekleştirir.

**Data:** Entity Framework Core aracılığıyla
veritabanı bağlantısını ve varlık ilişkilerini yönetir.

**Migrations:** Veritabanı şemasındaki
değişiklikleri takip eder.

## Veritabanı

Projede PostgreSQL veritabanı kullanılmaktadır.

Veritabanı işlemleri Entity Framework Core
üzerinden gerçekleştirilmektedir.

Temel veri yapılarından bazıları:

- Kurum
- Şube
- Poliklinik
- Doktor
- Sağlık Personeli
- Doktor Çalışma Saati
- Hastane Çalışma Saati
- Personel Hizmeti
- Randevu
- Kullanıcı ve Rol Bilgileri

Kullanıcı kimlik doğrulama ve yetkilendirme
işlemleri ASP.NET Core Identity ile yönetilir.

## Kurulum

### Gereksinimler

- .NET 10 SDK
- PostgreSQL
- Git
- Visual Studio Code veya uyumlu bir IDE
- Entity Framework Core CLI aracı

### 1. Projeyi Klonlayın

```bash
git clone https://github.com/yusuftahaezgin/HastaneRandevuSistemi.git
cd HastaneRandevuSistemi
```

### 2. Bağımlılıkları Yükleyin

```bash
dotnet restore
```

### 3. PostgreSQL Bağlantısını Yapılandırın

PostgreSQL sunucusunun çalıştığından emin olun.

Geliştirme ortamında veritabanı bağlantısı
.NET User Secrets üzerinden tanımlanabilir.

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=HastaneRandevuSistemi;Username=postgres;Password=VERITABANI_SIFRENIZ"
```

`VERITABANI_SIFRENIZ` bölümünü kendi PostgreSQL
parolanızla değiştirin.

### 4. Yönetici Hesabını Yapılandırın

Uygulama, yapılandırma bilgileri sağlandığında
başlangıçta yönetici hesabı oluşturabilir.

```bash
dotnet user-secrets set "Admin:UserName" "admin"
dotnet user-secrets set "Admin:Email" "admin@ornek.com"
dotnet user-secrets set "Admin:Password" "KENDI_GUCLU_PAROLANIZ"
```

E-posta adresini ve parolayı kendinize göre
değiştirin. Parola, Identity güvenlik
kurallarını karşılamalıdır.

### 5. Veritabanını Oluşturun/Güncelleyin

Entity Framework CLI aracı yüklü değilse:

```bash
dotnet tool install --global dotnet-ef --version 10.0.14
```

Ardından mevcut migration dosyalarını uygulayın:

```bash
dotnet ef database update
```

PostgreSQL kullanıcısının gerekli veritabanı
oluşturma ve güncelleme izinlerine sahip
olması gerekir.

### 6. Uygulamayı Çalıştırın

```bash
dotnet run
```

Yerel geliştirme profilinde örnek uygulama adresi:

```text
http://localhost:5256
```

## Test Senaryoları

Proje geliştirme sürecinde aşağıdaki
senaryolar manuel olarak test edilmiştir:

- Normal randevu oluşturma
- Aynı doktor için çakışan randevuların engellenmesi
- Aynı hastanın çakışan randevularının engellenmesi
- Doktor çalışma saatleri dışında randevu kontrolü
- Hastane kapalıyken randevu oluşturma kontrolü
- Geçmiş tarihe randevu alma kontrolü
- Yönetici tarafından randevu onaylama
- Yönetici tarafından randevu reddetme
- Pasif doktor için randevu kontrolü
- Sağlık personeli hizmet randevusu
- Kullanıcı rollerine göre erişim kontrolü
- Doktor parolasının yönetici tarafından sıfırlanması

Bu senaryolar yerel geliştirme ortamında
manuel olarak doğrulanmıştır.

## Güvenlik

- ASP.NET Core Identity ile kullanıcı yönetimi
- Rol tabanlı yetkilendirme
- Parolaların hash olarak saklanması
- Yönetici işlemlerinde erişim kontrolü
- Form işlemlerinde antiforgery doğrulaması
- Randevu oluşturma sırasında sunucu tarafı kontroller
- PostgreSQL tabanlı randevu çakışma kontrolleri

Veritabanı parolaları ve yönetici hesap
bilgileri GitHub deposuna eklenmemelidir.

## Proje Bilgileri

**Proje Adı:** Hastane Randevu Sistemi

**Ders:** Nesneye Dayalı Programlama Dilleri

**Üniversite:** Sakarya Üniversitesi

**Program:** Bilgisayar Mühendisliği Yüksek Lisans

**Geliştirici:** Yusuf Taha Ezgin

**Yıl:** 2026

**GitHub:** https://github.com/yusuftahaezgin/HastaneRandevuSistemi

---

Bu proje eğitim amacıyla geliştirilmiştir.
