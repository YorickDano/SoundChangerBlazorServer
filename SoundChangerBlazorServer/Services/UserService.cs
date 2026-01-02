using System.Security.Claims;

namespace SoundChangerBlazorServer.Services
{
    public class UserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentUserId()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context.User.Identity.IsAuthenticated)
            {
                return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            context.Request.Cookies.TryGetValue("AnonymousId", out var anonymousId);
            return anonymousId;
        }
    }
}
