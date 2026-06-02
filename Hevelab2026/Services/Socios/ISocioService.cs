using Hevelab2026.Common;
using Hevelab2026.DTOs.Socios;
using Hevelab2026.Models;

namespace Hevelab2026.Services.Socios;

public interface ISocioService
{
    Task<PagedResult<SocioResponseDto>> GetClientesAsync(PagedQuery query, string? estado, CancellationToken ct = default);
    Task<SocioResponseDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SocioResponseDto> CreateClienteAsync(SocioCreateDto dto, int empresaId = 1, CancellationToken ct = default);
    Task<SocioResponseDto> UpdateClienteAsync(int id, SocioUpdateDto dto, CancellationToken ct = default);
    Task ToggleActivoAsync(int id, CancellationToken ct = default);
    Cliente ToViewModel(SocioResponseDto dto);
    SocioResponseDto FromClienteForm(Cliente cliente);
}
