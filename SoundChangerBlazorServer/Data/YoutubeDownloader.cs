using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MediaToolkit.Model;
using MediaToolkit.Options;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using SoundChangerBlazorServer.Models;
using System.Net.Http;
using VideoLibrary;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace SoundChangerBlazorServer.Data
{
    public class YoutubeDownloader
    {
        private readonly YoutubeClient youtubeClient;
        private readonly YouTubeService YouTubeService;

        public YoutubeDownloader(IOptions<YoutubeApiSettings> youtubeOptions,
            IWebHostEnvironment webHostEnvironment)
        {
            youtubeClient = new YoutubeClient();
            YouTubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = youtubeOptions.Value.ApiKey
            });
        }

        public async Task<(string, string)> Download(string videoUrl, string path)
        {

            var videoInfo = await youtubeClient.Videos.GetAsync(videoUrl);
            var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoUrl);
            var streamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();

            var restrictedCharacters = new char[] { '\\', '/', ':', '*', '?', '\"', '<', '>', '|', '+' };
            var videoTitle = restrictedCharacters.Aggregate(videoInfo.Title, (c1, c2) => c1.Replace(c2, ' '));

            var filePath = Path.Combine(path, videoTitle + ".mp4");
            await youtubeClient.Videos.Streams.DownloadAsync(streamInfo, filePath);

            return (videoTitle, filePath);
        }

        public async Task<YoutubeExplode.Videos.Video> GetVideoInfoAsync(string url)
        {
            return await youtubeClient.Videos.GetAsync(url);
        }

        public async Task<string> GetVideoIdByTitleAsync(string titleAuthor)
        {
            var searchListRequest = YouTubeService.Search.List("snippet");
            searchListRequest.Q = titleAuthor;
            searchListRequest.MaxResults = 1;

            var searchListResponse = await searchListRequest.ExecuteAsync();

            return searchListResponse.Items[0].Id.VideoId;
        }
    }
}
