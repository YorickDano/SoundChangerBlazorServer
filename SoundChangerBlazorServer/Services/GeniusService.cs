using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using RestSharp;
using SoundChangerBlazorServer.Models.GeniusModels;
using SoundChangerBlazorServer.Utils;
using Swan;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SoundChangerBlazorServer.Services
{
    public class GeniusService
    {
        private readonly RestClient _client;
        private readonly GeniusSettings _settings;
        private readonly IJSRuntime _jsRuntime;
        private readonly StateContainer _stateContainer;
        public bool IsAuthorized { get; private set; }

        public GeniusService(IOptions<GeniusSettings> options, IJSRuntime jSRuntime,
                             StateContainer stateContainer)
        {
            _settings = options.Value;
            _client = new RestClient(_settings.BaseUrl);
            _jsRuntime = jSRuntime;
            _stateContainer = stateContainer;
        }

        public async Task<string> SerchForLyrics(string title)
        {
            var request = new RestRequest("search", Method.Get);
            request.AddParameter("q", title);
            request.AddParameter("access_token", _stateContainer.GeniusToken);
            var response = await _client.ExecuteAsync(request);
            try
            {
                var genieRes = JsonSerializer.Deserialize<GeniusSearchResponse.Root>(response.Content);
                var lyricsPath = genieRes.response.hits?[0].result.path;

                var _httpClient = new HttpClient();
                var res = await _httpClient.GetAsync($"https://genius.com{lyricsPath}");
                string htmlContent = await res.Content.ReadAsStringAsync();
                var parser = new GeniusParser();
                return parser.Parse(htmlContent);
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public async Task Authorize()
        {
            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = _settings.ClientId,
                ["redirect_uri"] = _settings.RedirectUrl,
                ["response_type"] = "code"
            };

            var queryString = string.Join("&", parameters
                .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

            await _jsRuntime.InvokeVoidAsync("open", Path.Combine(_settings.BaseUrl, "oauth", $"authorize?{queryString}"), "_blank");
        }

        public async Task<string> Authorize(string code)
        {
            var authRequest = new RestRequest("oauth/token", Method.Post);
            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = _settings.ClientId,
                ["client_secret"] = _settings.ClientSecret,
                ["redirect_uri"] = _settings.RedirectUrl,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["response_type"] = "code"
            };
            authRequest.AddJsonBody(parameters.ToJson());
            var response = await _client.ExecuteAsync<GeniusAuthResponse>(authRequest);
            var geniusToken = JsonSerializer.Deserialize<GeniusAuthResponse>(response.Content);

            return geniusToken.AccessToken;
        }

        public class GeniusAuthResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }
        }
    }
}
