using System.Security.Claims;
using MyLibrary.Application.Interfaces;

namespace MyLibrary.Api.Services;

/// <summary>
/// Resolves the current user's id from the <see cref="ClaimTypes.NameIdentifier"/> claim
/// of the authenticated <see cref="HttpContext.User"/>, as set by <c>JwtTokenService</c>.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }
}
