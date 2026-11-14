using HastaneRandevuSistemi.Models;

namespace HastaneRandevuSistemi.Services.Interfaces;

public interface IDoktorCalismaSaatiService
{
    Task<List<DoktorCalismaSaati>> ListeleAsync(
        int? doktorId = null);

    Task<DoktorCalismaSaati?> GetirAsync(int id);

    Task<List<Doktor>> SecilebilirDoktorlariListeleAsync();

    Task EkleAsync(DoktorCalismaSaatiFormViewModel model);

    Task<bool> GuncelleAsync(
        DoktorCalismaSaatiFormViewModel model);

    Task<bool> DurumDegistirAsync(int id);
}