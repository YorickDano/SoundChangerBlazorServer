using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SoundChangerBlazorServer.Services.Interfaces;
using SpotifyAPI.Web;

namespace SoundChangerBlazorServer.Services.YoutubeServices
{
    public class YoutubeMusicService
    {
        private readonly IConfiguration _configuration;
        private readonly IJSRuntime _jsRuntime;
        private readonly ITokenStorageService _tokenStorageService;
        private YouTubeService _youtubeService;
        private readonly UserService _userService;

        public bool IsAuthorized { get; private set; } = false;

        public YoutubeMusicService(IConfiguration configuration, IJSRuntime jSRuntime,
                                   ITokenStorageService tokenStorageService, UserService userService)
        {
            _configuration = configuration;
            _jsRuntime = jSRuntime;
            _tokenStorageService = tokenStorageService;
            _userService = userService;
            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = configuration["YoutubeApiSettings:ApiKey"],
                ApplicationName = "YouTubeMusicApp"
            });
        }

        public async Task Authorize()
        {
            var url = await GenerateAuthUrl();

            await _jsRuntime.InvokeVoidAsync("open", url, "_blank");
        }

        public async Task<List<YoutubeTrack>> GetMostPolularTracksAsync()
        {
            var tracks = new List<YoutubeTrack>();

            try
            {
                var searchRequest = _youtubeService.Videos.List("snippet");
                searchRequest.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
                searchRequest.VideoCategoryId = "10";
                searchRequest.MaxResults = 50;
                searchRequest.AccessToken = (await _tokenStorageService.GetTokensAsync(_userService.GetCurrentUserId())).AccessToken;

                var searchResponse = await searchRequest.ExecuteAsync();

                return await VideoResponseToTracks(searchResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting recommended tracks: {ex.Message}");
                throw;
            }
        }

        public async Task<(List<YoutubeTrack> tracks, string nextPageToken)> GetLikedTracksAsync(string? nextPageToken = null, int maxResults = 10)
        {
            try
            {
                var searchRequest = _youtubeService.Videos.List("snippet");
                searchRequest.MyRating = VideosResource.ListRequest.MyRatingEnum.Like;
                searchRequest.VideoCategoryId = "10";
                searchRequest.PageToken = nextPageToken;
                searchRequest.MaxResults = maxResults;
                searchRequest.AccessToken = (await _tokenStorageService.GetTokensAsync(_userService.GetCurrentUserId())).AccessToken;

                var searchResponse = await searchRequest.ExecuteAsync();

                return (await VideoResponseToTracks(searchResponse), searchResponse.NextPageToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting recommended tracks: {ex.Message}");
                throw;
            }
        }

        private async Task<List<YoutubeTrack>> VideoResponseToTracks(VideoListResponse response)
        {
            var tracks = new List<YoutubeTrack>();
            foreach (var searchResult in response.Items.Where(i => i.Snippet.CategoryId == "10"))
            {
                var track = new YoutubeTrack
                {
                    Id = searchResult.Snippet.Thumbnails.Default__.Url.Split('/')[^2],
                    Title = searchResult.Snippet.Title,
                    ChannelTitle = searchResult.Snippet.ChannelTitle,
                    ThumbnailUrl = searchResult.Snippet.Thumbnails.Medium?.Url,
                    PublishedAt = searchResult.Snippet.PublishedAt
                };

                tracks.Add(track);
            }

            return tracks;
        }

        private async Task<string> GenerateAuthUrl()
        {
            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = _configuration["Google:ClientId"]!,
                ["redirect_uri"] = _configuration["Google:RedirectUri"]!,
                ["scope"] = _configuration["Google:Scope"]!,
                ["response_type"] = "code",
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

