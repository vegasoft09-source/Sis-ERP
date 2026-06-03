using Hevelab2026.Models;

namespace Hevelab2026.Services
{
    public interface IDashboardService
    {
        Task<List<MetricCard>> ObtenerMetricasAsync(int empresaId);
    }
}
