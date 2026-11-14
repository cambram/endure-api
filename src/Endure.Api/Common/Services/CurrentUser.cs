using System.Security.Claims;
using Endure.Application.Common.Services;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Endure.Api.Common.Services;

internal sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? AppUserId
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;

            if (principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            if (Guid.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out var appUserId))
            {
                return appUserId;
            }

            return null;
        }
    }
}
