namespace SoundChangerBlazorServer.Middleware
{
    public class UserMiddleware
    {
        private readonly RequestDelegate _next;

        public UserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                if (!context.Request.Cookies.TryGetValue("AnonymousId", out _))
                {
                    var anonymousId = Guid.NewGuid().ToString();
                    context.Response.Cookies.Append(
                        "AnonymousId",
                        anonymousId,
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Expires = DateTime.UtcNow.AddYears(1)
                        });
                }
            }

            await _next(context);
        }
    }
}