using Hevelab2026.Models;

namespace Hevelab2026.Services.Dashboard;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync(CancellationToken ct = default);
}
