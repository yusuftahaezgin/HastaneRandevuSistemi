using HastaneRandevuSistemi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC servislerini ekle
builder.Services.AddControllersWithViews();

// PostgreSQL bağlantı ayarını al
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "PostgreSQL bağlantı ayarı bulunamadı.");

// Entity Framework Core ve PostgreSQL bağlantısını kaydet
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Uygulamayı oluştur
var app = builder.Build();

// HTTP isteklerini yapılandır
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();