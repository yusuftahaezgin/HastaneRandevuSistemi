using HastaneRandevuSistemi.Models;

namespace HastaneRandevuSistemi.Services.Interfaces;

public interface ISaglikPersoneliService
{
    Task<List<SaglikPersoneli>> ListeleAsync();

    Task<SaglikPersoneli?> GetirAsync(int id);

    Task EkleAsync(SaglikPersoneliEkleViewModel model);

    Task<bool> GuncelleAsync(SaglikPersoneliDuzenleViewModel model);

    Task<bool> DurumDegistirAsync(int id);
}