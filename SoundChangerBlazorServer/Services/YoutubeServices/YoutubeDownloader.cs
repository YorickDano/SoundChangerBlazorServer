using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Options;
using SoundChangerBlazorServer.Models.YoutubeModels;
using SoundChangerBlazorServer.Services.Interfaces;
using YoutubeDLSharp;

namespace SoundChangerBlazorServer.Services.YoutubeServices
{
    public class YoutubeDownloader : IYoutubeDownloader
    {
        private readonly YouTubeService _youTubeService;
        private readonly YoutubeDL _youtubeDL;

        private readonly string BaseVideoUrl;
        private readonly string BasePlaylistUrl;
        private readonly string WebRootPath;

        public YoutubeDownloader(IOptions<YoutubeApiSettings> youtubeOptions, IWebHostEnvironment hostEnvironment,
                                 IOptions<YoutubeDownloaderSettings> youtubeDownloadSettings, IHttpClientFactory clientFactory)
        {
            _youTubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = youtubeOptions.Value.ApiKey
            });
            BaseVideoUrl = youtubeDownloadSettings.Value.BaseVideoUrl;
            BasePlaylistUrl = youtubeDownloadSettings.Value.BasePlaylistUrl;
            WebRootPath = hostEnvironment.WebRootPath;
            _youtubeDL = new YoutubeDL();
            _youtubeDL.YoutubeDLPath = Path.Combine(WebRootPath, "yt-dlp.exe");
            _youtubeDL.FFmpegPath = Path.Combine(WebRootPath, "ffmpeg.exe");
            _youtubeDL.OutputFolder = WebRootPath;
        }

        public async Task<(YoutubeVideo?, string)> Download(string videoId)
        {
            var result = await _youtubeDL.RunAudioDownload(BaseVideoUrl + videoId, YoutubeDLSharp.Options.AudioConversionFormat.Wav);
            var directoryInfo = new DirectoryInfo(WebRootPath);
            var file = directoryInfo.GetFiles()
                                    .FirstOrDefault(f => f.Name.Contains(videoId));

            if (file == null)
            {
                return (null, string.Empty);
            }

            var video = await GetVideo(videoId);

            return (video, file!.FullName);
        }

        public async Task<YoutubeVideo> GetVideo(string id)
        {
            var search = _youTubeService.Videos.List("snippet");
            search.Id = id;
            var res = await search.ExecuteAsync();

            return new YoutubeVideo()
            {
                Title = res.Items[0].Snippet.Title,
                Url = BaseVideoUrl + res.Items[0].Id,
                ImgUrl = res.Items[0].Snippet.Thumbnails.Medium.Url,
                Id = res.Items[0].Id,
                ChannelTitle = res.Items[0].Snippet.ChannelTitle
            };
        }

        public async Task<IEnumerable<YoutubeVideo>> GetVideosAsync(string query)
        {
            var search = _youTubeService.Search.List("snippet");
            search.Q = query;
            search.MaxResults = 20;

            var response = await search.ExecuteAsync();

            return response.Items.Select(x => new YoutubeVideo()
            {
                Title = x.Snippet.Title,
                Url = BaseVideoUrl + x.Id.VideoId,
                ImgUrl = x.Snippet.Thumbnails.Medium.Url,
                Id = x.Id.VideoId
            });
        }

        public async Task<YoutubePlaylist> GetPlaylistAsync(string query)
        {
            var youtubePlaylist = new YoutubePlaylist();

            var searchListResponse = await GetSearchRequest(query).ExecuteAsync();
            var playlistId = searchListResponse.Items[0].Id.PlaylistId;
            var result = await _youtubeDL.RunVideoDataFetch(BasePlaylistUrl + playlistId);
            youtubePlaylist.Videos = result.Data.Entries.Select(x => new YoutubeVideo
            {
                Id = x.ID,
                Title = x.Title,
                Url = x.Url,
                ImgUrl = x.Thumbnails[1].Url
            });
            return youtubePlaylist;
        }

        private SearchResource.ListRequest GetSearchRequest(string query)
        {
            var searchRequest = _youTubeService.Search.List("snippet");
            searchRequest.Q = query;
            searchRequest.MaxResults = 5;

            return searchRequest;
        }
    }
}
