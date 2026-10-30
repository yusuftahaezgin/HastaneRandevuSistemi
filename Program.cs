using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "PostgreSQL bağlantı ayarı bulunamadı.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;

    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Giris";
    options.AccessDeniedPath = "/Account/ErisimYok";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(3);
    options.SlidingExpiration = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Yerel geliştirme için yönetici hesabı
var adminUserName = builder.Configuration["Admin:UserName"];
var adminEmail = builder.Configuration["Admin:Email"];
var adminPassword = builder.Configuration["Admin:Password"];

if (!string.IsNullOrWhiteSpace(adminUserName)
    && !string.IsNullOrWhiteSpace(adminEmail)
    && !string.IsNullOrWhiteSpace(adminPassword))
{
    using var scope = app.Services.CreateScope();

    var userManager =
        scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var admin = await userManager.FindByNameAsync(adminUserName);

    if (admin is null)
    {
        admin = await userManager.FindByEmailAsync(adminEmail);
    }

    if (admin is null)
    {
        admin = new ApplicationUser
        {
            UserName = adminUserName,
            Email = adminEmail,
            Ad = "Hastane",
            Soyad = "Yöneticisi",
            EmailConfirmed = true
        };

        var createResult =
            await userManager.CreateAsync(admin, adminPassword);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                createResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Yönetici hesabı oluşturulamadı: {errors}");
        }
    }

    if (!await userManager.IsInRoleAsync(admin, "Yonetici"))
    {
        var roleResult =
            await userManager.AddToRoleAsync(admin, "Yonetici");

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Yönetici rolü atanamadı.");
        }
    }
}

app.Run();