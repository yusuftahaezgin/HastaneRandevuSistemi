using HastaneRandevuSistemi.Models;

namespace HastaneRandevuSistemi.Services.Interfaces;

public interface IDoktorService
{
    Task<List<Doktor>> ListeleAsync();

    Task<Doktor?> GetirAsync(int id);

    Task EkleAsync(DoktorEkleViewModel model);

    Task<bool> GuncelleAsync(DoktorDuzenleViewModel model);

    Task<bool> DurumDegistirAsync(int id);
}