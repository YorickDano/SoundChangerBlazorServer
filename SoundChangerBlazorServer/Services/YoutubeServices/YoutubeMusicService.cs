using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SoundChangerBlazorServer.Services.YoutubeServices
{
    public class YoutubeMusicService
    {
        private YouTubeService _youtubeService;
        private readonly IConfiguration _configuration;
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public YoutubeMusicService(IConfiguration configuration, NavigationManager navigationManager,
                                   IJSRuntime jSRuntime, IHttpContextAccessor httpContextAccessor)
        {
            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = configuration["YouTube:ApiKey"],
                ApplicationName = "YouTubeMusicApp"
            });
            _configuration = configuration;
            _navigationManager = navigationManager;
            _jsRuntime = jSRuntime;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Authorize()
        {
            var url = await GenerateAuthUrl();

            await _jsRuntime.InvokeVoidAsync("open", url, "_blank");
        }

        public void InitializeWithToken(string accessToken)
        {
            var credential = GoogleCredential.FromAccessToken(accessToken);

            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "YouTubeMusicApp"
            });
        }

        public async Task<(List<YoutubeTrack> tracks, string nextPageToken)> GetLikedTracksAsync(string? nextPageToken = null, int maxResults = 10)
        {
            var recommendedTracks = new List<YoutubeTrack>();

            try
            {
                var searchRequest = _youtubeService.Videos.List("snippet");
                searchRequest.MyRating = VideosResource.ListRequest.MyRatingEnum.Like;
                searchRequest.VideoCategoryId = "10";
                searchRequest.PageToken = nextPageToken;
                searchRequest.MaxResults = maxResults;
                searchRequest.AccessToken = _httpContextAccessor.HttpContext!.Session.GetString("YouTubeAccessToken");

                var searchResponse = await searchRequest.ExecuteAsync();

                foreach (var searchResult in searchResponse.Items.Where(i => i.Snippet.CategoryId == "10"))
                {
                    var track = new YoutubeTrack
                    {
                        Id = searchResult.Snippet.Thumbnails.Default__.Url.Split('/')[^2],
                        Title = searchResult.Snippet.Title,
                        ChannelTitle = searchResult.Snippet.ChannelTitle,
                        ThumbnailUrl = searchResult.Snippet.Thumbnails.Medium?.Url,
                        PublishedAt = searchResult.Snippet.PublishedAt
                    };

                    recommendedTracks.Add(track);

                }

                return (recommendedTracks, searchResponse.NextPageToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting recommended tracks: {ex.Message}");
                throw;
            }
        }

        private async Task<string> GenerateAuthUrl()
        {
            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = _configuration["Google:ClientId"],
                ["redirect_uri"] = _configuration["Google:Callback"],
                ["response_type"] = "code",
                ["scope"] = _configuration["Google:Scope"],
                ["prompt"] = "consent",
                ["include_granted_scopes"] = "true",
                ["access_type"] = "offline",
                ["prompt"] = "consent"
            };

            var queryString = string.Join("&", parameters
                .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

            return $"{_configuration["Google:AuthUrl"]}{queryString}";
        }
    }

    public class YoutubeTrack
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ChannelTitle { get; set; }
        public string ThumbnailUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public ulong? ViewCount { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
}

