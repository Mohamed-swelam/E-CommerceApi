using System.Security.Claims;

namespace Core.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        }
    }
}
