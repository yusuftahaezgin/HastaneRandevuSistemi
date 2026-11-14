using HastaneRandevuSistemi.Models;

namespace HastaneRandevuSistemi.Services.Interfaces;

public interface IPoliklinikService
{
    Task<List<Poliklinik>> ListeleAsync();

    Task<Poliklinik?> GetirAsync(int id);

    Task EkleAsync(
        int subeId,
        string ad,
        string? aciklama,
        bool aktifMi);

    Task<bool> GuncelleAsync(
        int id,
        int subeId,
        string ad,
        string? aciklama,
        bool aktifMi);

    Task<bool> DurumDegistirAsync(int id);
}