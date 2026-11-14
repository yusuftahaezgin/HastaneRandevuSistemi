using System.Diagnostics;
using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(
        ILogger<HomeController> logger,
        AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var poliklinikler = await _context.Poliklinikler
            .AsNoTracking()
            .Include(p => p.Sube)
                .ThenInclude(s => s!.Kurum)
            .Where(p =>
                p.AktifMi
                && p.Sube != null
                && p.Sube.AktifMi
                && p.Sube.Kurum != null
                && p.Sube.Kurum.AktifMi)
            .OrderBy(p => p.Sube!.Kurum.Ad)
            .ThenBy(p => p.Sube!.Ad)
            .ThenBy(p => p.Ad)
            .ToListAsync();

        return View(poliklinikler);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
        });
    }
}