using System.Security.Claims;

namespace Hevelab2026.Services.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http) => _http = http;

    public bool IsAuthenticated => _http.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public int? UserId
    {
        get
        {
            var id = _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(id, out var uid) ? uid : null;
        }
    }

    public int EmpresaId
    {
        get
        {
            var eid = _http.HttpContext?.User?.FindFirstValue("EmpresaId");
            return int.TryParse(eid, out var id) ? id : 1;
        }
    }
}
