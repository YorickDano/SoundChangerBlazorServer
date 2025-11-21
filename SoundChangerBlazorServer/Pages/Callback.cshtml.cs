using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SoundChangerBlazorServer.Services.YoutubeServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SoundChangerBlazorServer.Pages
{
    public class CallbackModel : PageModel
    {
        private readonly YoutubeMusicService _youtubeMusicService;
        private readonly IConfiguration _configuration;
        private readonly NavigationManager _navigationManager;
        public CallbackModel(YoutubeMusicService youtubeMusicService, 
                             IConfiguration configuration, NavigationManager navigationManager)
        {
            _youtubeMusicService = youtubeMusicService;
            _configuration = configuration;
            _navigationManager = navigationManager;
        }
        
        public async Task<IActionResult> OnGetAsync(string code, string scope)
        {
            var tokenResponse = await ExchangeCodeForTokenAsync(code);

            // Инициализируйте сервис с токеном
            _youtubeMusicService.InitializeWithToken(tokenResponse.AccessToken);

            // Сохраните токен (в сессии, базе данных, etc)
            HttpContext.Session.SetString("YouTubeAccessToken", tokenResponse.AccessToken);
            HttpContext.Session.SetString("YouTubeRefreshToken", tokenResponse.RefreshToken);
         
            return Page();
        }

        private async Task<TokenResponse> ExchangeCodeForTokenAsync(string code)
        {
            using var httpClient = new HttpClient();
            var redirectUri = Path.Combine("https://localhost:7242/", "callback");

            var tokenRequest = new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = _configuration["Google:ClientId"],
                ["client_secret"] = _configuration["Google:ClientSecret"],
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code",
                ["access_type"] = "offline",
                ["prompt"] = "consent"
            };

            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(tokenRequest));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Token exchange failed: {response.StatusCode} - {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TokenResponse>(jsonResponse);
        }

        public class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonPropertyName("scope")]
            public string Scope { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }
        }
    }
}
