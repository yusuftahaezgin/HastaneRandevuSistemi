using HastaneRandevuSistemi.Models;

namespace HastaneRandevuSistemi.Services.Interfaces;

public interface IKurumService
{
    Task<List<Kurum>> ListeleAsync();

    Task<Kurum?> GetirAsync(int id);

    Task EkleAsync(
        string ad,
        string? aciklama,
        bool aktifMi);

    Task<bool> GuncelleAsync(
        int id,
        string ad,
        string? aciklama,
        bool aktifMi);

    Task<bool> DurumDegistirAsync(int id);
}