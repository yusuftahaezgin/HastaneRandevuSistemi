using HastaneRandevuSistemi.Models;

namespace HastaneRandevuSistemi.Services.Interfaces;

public interface ISubeService
{
    Task<List<Sube>> ListeleAsync();

    Task<Sube?> GetirAsync(int id);

    Task EkleAsync(
        int kurumId,
        string ad,
        string adres,
        string? telefon,
        bool aktifMi);

    Task<bool> GuncelleAsync(
        int id,
        string ad,
        string adres,
        string? telefon,
        bool aktifMi);

    Task<bool> DurumDegistirAsync(int id);
}