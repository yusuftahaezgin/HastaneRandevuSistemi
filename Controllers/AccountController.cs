using HastaneRandevuSistemi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevuSistemi.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Giris(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Giris(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var identifier = model.KullaniciAdiVeyaEposta.Trim();

        var user = await _userManager.FindByNameAsync(identifier);

        if (user is null)
            user = await _userManager.FindByEmailAsync(identifier);

        if (user is null)
        {
            ModelState.AddModelError("", "Kullanıcı adı veya parola hatalı.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.BeniHatirla,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl)
                && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            if (await _userManager.IsInRoleAsync(user, "Hasta"))
                return RedirectToAction("Olustur", "Randevu");

            if (await _userManager.IsInRoleAsync(user, "Yonetici"))
                return RedirectToAction("Index", "Home");

            if (await _userManager.IsInRoleAsync(user, "Doktor"))
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(
            "",
            result.IsLockedOut
                ? "Çok fazla başarısız giriş yapıldı. Daha sonra tekrar deneyin."
                : "Kullanıcı adı veya parola hatalı.");

        return View(model);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult UyeOl(string? returnUrl = null)
    {
        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UyeOl(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            Ad = model.Ad.Trim(),
            Soyad = model.Soyad.Trim(),
            Email = model.Email.Trim(),
            UserName = model.Email.Trim()
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Hasta");

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            ModelState.AddModelError("", "Hasta rolü atanamadı.");
            return View(model);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl)
            && Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction("Olustur", "Randevu");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cikis()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ErisimYok()
    {
        return View();
    }
}