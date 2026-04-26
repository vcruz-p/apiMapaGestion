using Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class CurrentContextService : ICurrentContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 1; // Default to 1 for demo
        }
    }

    public int OrganizationId
    {
        get
        {
            var orgIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("organization_id")?.Value;
            return int.TryParse(orgIdClaim, out var orgId) ? orgId : 1; // Default to 1 for demo
        }
    }
}
