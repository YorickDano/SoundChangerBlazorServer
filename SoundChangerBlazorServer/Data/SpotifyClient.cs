using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using SoundChangerBlazorServer.Models.SpotifyModels;
using System.Text;

namespace SoundChangerBlazorServer.Data
{
    public class SpotifyClient
    {
        private SpotifyClientSettings Settings { get; set; }
        private RestClient RestClient { get; set; }
        private RestClient AuthorizeClient { get; set; }
        private SpotifyToken Token { get; set; }
        private Timer Timer { get; set; }
        public bool IsAuthorized { get; private set; } = false;

        private string AuthorizeUrl = "https://accounts.spotify.com/";
        private string RedirectUri = "https://localhost:7240/callback";

        public SpotifyClient(IOptions<SpotifyClientSettings> settings)
        {
            RestClient = new RestClient(settings.Value.BaseUrl);
            AuthorizeClient = new RestClient(AuthorizeUrl);
            Settings = settings.Value;
        }

        public async Task PlayPlayer()
        {
            var playerRequest = new RestRequest("v1/me/player/play", Method.Put);

            await RestClient.ExecuteAsync(playerRequest);
        }

        public async Task PausePlayer()
        {
            var playerRequest = new RestRequest("v1/me/player/pause", Method.Put);

            await RestClient.ExecuteAsync(playerRequest);
        }

        public async Task GetPlayer()
        {
            var playerRequest = new RestRequest("v1/me/player");

            var response = RestClient.Get(playerRequest);

            var player = JsonConvert.DeserializeObject<SpotifyPlayerResponse.Root>(response.Content);
        }

        public async Task<string> Authorize()
        {
            var restRequest = new RestRequest("authorize?");
            restRequest.AddHeader("Accept",
              "text/html, application/xhtml+xml, application/json, application/xml;q=0.9, image/webp, */*;q=0.8");
            restRequest.AddQueryParameter("response_type", "code");
            restRequest.AddQueryParameter("client_id", Settings.ClientId);
            restRequest.AddQueryParameter("scope", "user-read-private user-read-email user-read-playback-state user-modify-playback-state");
            restRequest.AddQueryParameter("redirect_uri", RedirectUri);

            var response = await AuthorizeClient.GetAsync(restRequest);
            var uri = response.ResponseUri;
            IsAuthorized = true;
            if (uri != null)
            {
                return uri.ToString();
            }

            return string.Empty;
        }

        public async Task<SpotifySearch.Root> SearchFor(string title, SpotifyTypes type = SpotifyTypes.track)
        {
            var restRequest = new RestRequest("/v1/search", Method.Get);
            restRequest.AddQueryParameter("q", title);
            restRequest.AddQueryParameter("type", type);
            restRequest.AddQueryParameter("limit", 1);

            var resp = await RestClient.ExecuteAsync(restRequest);

            return JsonConvert.DeserializeObject<SpotifySearch.Root>(resp.Content);
        }

        public async Task<IEnumerable<SpotifyPlaylist.Item>> ConvertPlaylist(string id)
        {
            var restRequest = new RestRequest($"/v1/playlists/{id}", Method.Get);

            var resp = await RestClient.ExecuteAsync(restRequest);

            var playlist = JsonConvert.DeserializeObject<SpotifyPlaylist.Root>(resp.Content);

            return playlist.tracks.items;
        }

        public async Task<SpotifyTrack.Root> ConvertTrack(string id)
        {
            var restRequest = new RestRequest($"/v1/tracks/{id}", Method.Get);

            var resp = await RestClient.ExecuteAsync(restRequest);

            var track = JsonConvert.DeserializeObject<SpotifyTrack.Root>(resp.Content);

            return track;
        }

        public async Task SetToken(string code)
        {
            var restRequest = new RestRequest("api/token", method: Method.Post);
            var bytes = Encoding.UTF8.GetBytes($"{Settings.ClientId}:{Settings.ClientSecret}");
            restRequest.AddHeader("Accept",
             "text/html, application/xhtml+xml, application/json, application/xml;q=0.9, image/webp, */*;q=0.8");
            restRequest.AddHeader("Authorization", "Basic " + Convert.ToBase64String(bytes));
            restRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            var spotifyAuth = new SpotifyAuth()
            {
                code = code,
                grant_type = "authorization_code",
                redirect_uri = RedirectUri
            };
            restRequest.AddObject(spotifyAuth);

            var response = await AuthorizeClient.ExecuteAsync(restRequest);

            if (response.Content != null)
            {
                Token = JsonConvert.DeserializeObject<SpotifyToken>(response.Content) ?? new SpotifyToken();
                RestClient.AddDefaultHeader("Authorization", $"Bearer {Token?.access_token}");

                var time = TimeSpan.FromSeconds(Token.expires_in);

                Timer = new Timer((e) => 
                { 
                    RefreshToken(); 
                }, null, time, time);
                IsAuthorized = true;
            }
        }

        private void RefreshToken()
        {
            Console.WriteLine(DateTime.Now);
            var restRequest = new RestRequest("api/token", Method.Post);

            var bytes = Encoding.UTF8.GetBytes($"{Settings.ClientId}:{Settings.ClientSecret}");

            var refreshToken = new SpotifyRefreshToken()
            {
                GrantType = "refresh_token",
                RefreshToken = Token.refresh_token,
                ClientId = Settings.ClientId
            };
            restRequest.AddHeader("Accept",
           "text/html, application/xhtml+xml, application/json, application/xml;q=0.9, image/webp, */*;q=0.8");
            restRequest.AddObject(refreshToken);
            restRequest.AddHeader("Authorization", "Basic " + Convert.ToBase64String(bytes));
            restRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");


            var response = AuthorizeClient.Execute(restRequest);

            Token = JsonConvert.DeserializeObject<SpotifyToken>(response.Content) ?? new SpotifyToken();
        }
    }
}
