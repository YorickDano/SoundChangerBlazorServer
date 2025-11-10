using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Options;
using SoundChangerBlazorServer.Models.YoutubeModels;
using SoundChangerBlazorServer.Services.Interfaces;
using System.IO;
using System.Net;
using YoutubeDLSharp;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace SoundChangerBlazorServer.Services
{
    public class YoutubeDownloader : IYoutubeDownloader
    {
        private readonly YoutubeClient _youtubeClient;
        private readonly YouTubeService _youTubeService;
        private readonly HttpClient _httpClient;
        private readonly YoutubeDL _youtubeDL;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private readonly string BaseVideoUrl;
        private readonly string BasePlaylistUrl;
        private readonly string WebRootPath;
        public YoutubeDownloader(IOptions<YoutubeApiSettings> youtubeOptions, IWebHostEnvironment hostEnvironment,
                                 IOptions<YoutubeDownloaderSettings> youtubeDownloadSettings, IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient(nameof(YoutubeClient));
            _youtubeClient = new YoutubeClient(_httpClient);
            _youTubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = youtubeOptions.Value.ApiKey
            });
            _hostingEnvironment = hostEnvironment;
            BaseVideoUrl = youtubeDownloadSettings.Value.BaseVideoUrl;
            BasePlaylistUrl = youtubeDownloadSettings.Value.BasePlaylistUrl;
            WebRootPath = _hostingEnvironment.WebRootPath;
            _youtubeDL = new YoutubeDL();
            _youtubeDL.YoutubeDLPath = WebRootPath + "\\yt-dlp.exe";
            _youtubeDL.FFmpegPath = WebRootPath + "\\ffmpeg.exe";
            _youtubeDL.OutputFolder = WebRootPath;
        }

        public async Task<(string, string)> Download(string videoId)
        {
            var result = await _youtubeDL.RunAudioDownload(BaseVideoUrl + videoId, YoutubeDLSharp.Options.AudioConversionFormat.Wav);
            var directoryInfo = new DirectoryInfo(WebRootPath);
            var file = directoryInfo.GetFiles().FirstOrDefault(f => f.Name.Contains(videoId));

            var fileName = Path.GetFileName(result.Data);

            return (Path.GetFileNameWithoutExtension(file!.Name), file!.FullName);
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

        public async Task<YoutubeVideo> GetVideoAsync(string query)
        {
            var youtubeVideo = new YoutubeVideo();
            var video = (IVideo)(query.Contains(BaseVideoUrl) ? await _youtubeClient.Videos.GetAsync(query) : (await _youtubeClient.Search.GetVideosAsync(query)).First());
            youtubeVideo.Id = video.Id;
            youtubeVideo.Url = video.Url;
            youtubeVideo.Title = video.Title;
            youtubeVideo.ImgUrl = video.Thumbnails[3].Url;

            return youtubeVideo;
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
