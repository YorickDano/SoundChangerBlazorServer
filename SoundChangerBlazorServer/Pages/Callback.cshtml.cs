using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SoundChangerBlazorServer.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SoundChangerBlazorServer.Pages
{
    public class CallbackModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly GeniusService _geniusService;
        private readonly StateContainer _stateContainer;

        public CallbackModel(IConfiguration configuration, GeniusService geniusService, 
                             StateContainer stateContainer)
        {
            _configuration = configuration;
            _geniusService = geniusService;
            _stateContainer = stateContainer;
        }

        public async Task<IActionResult> OnGetGeniusAsync(string code)
        {
            var token = await _geniusService.Authorize(code);
            _stateContainer.Token = token;

            return Page();
        }

        public async Task<IActionResult> OnGetYoutubeAsync(string code, string scope)
        {
            var tokenResponse = await ExchangeCodeForTokenAsync(code);

            HttpContext.Session.SetString("YouTubeAccessToken", tokenResponse.AccessToken);
            HttpContext.Session.SetString("YouTubeRefreshToken", tokenResponse.RefreshToken);

            return Page();
        }

        private async Task<TokenResponse> ExchangeCodeForTokenAsync(string code)
        {
            using var httpClient = new HttpClient();
            var redirectUri = _configuration["Google:Callback"] ?? throw new InvalidOperationException("Google:Callback is not configured.");
            var clientId = _configuration["Google:ClientId"] ?? throw new InvalidOperationException("Google:ClientId is not configured.");
            var clientSecret = _configuration["Google:ClientSecret"] ?? throw new InvalidOperationException("Google:ClientSecret is not configured.");

            var tokenRequest = new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code",
                ["access_type"] = "offline",
                ["prompt"] = "consent"
            };

            var response = await httpClient.PostAsync(_configuration["Google:TokenUrl"],
                new FormUrlEncodedContent(tokenRequest));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Token exchange failed: {response.StatusCode} - {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse);

            return tokenResponse ?? throw new Exception("Failed to deserialize token response.");
        }

        public class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public required string AccessToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("refresh_token")]
            public required string RefreshToken { get; set; }

            [JsonPropertyName("scope")]
            public required string Scope { get; set; }

            [JsonPropertyName("token_type")]
            public required string TokenType { get; set; }
        }
    }
}
