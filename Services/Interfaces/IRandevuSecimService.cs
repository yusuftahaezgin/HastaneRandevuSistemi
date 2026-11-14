using HastaneRandevuSistemi.Models;

namespace HastaneRandevuSistemi.Services.Interfaces;

public interface IRandevuSecimService
{
    Task<RandevuSecimViewModel> HazirlaAsync(
        int? kurumId = null,
        int? subeId = null,
        int? poliklinikId = null,
        int? doktorId = null);
}