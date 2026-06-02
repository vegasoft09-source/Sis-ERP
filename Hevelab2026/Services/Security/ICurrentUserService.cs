namespace Hevelab2026.Services.Security;

public interface ICurrentUserService
{
    int? UserId { get; }
    int EmpresaId { get; }
    bool IsAuthenticated { get; }
}
