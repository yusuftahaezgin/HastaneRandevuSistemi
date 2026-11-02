using HastaneRandevuSistemi.Models;

namespace HastaneRandevuSistemi.Services.Interfaces;

public interface IRandevuSecimService
{
    Task<RandevuSecimViewModel?> HazirlaAsync(
        int? poliklinikId,
        int? doktorId);
}