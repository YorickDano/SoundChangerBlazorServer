using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Options;
using SoundChangerBlazorServer.Models.YoutubeModels;
using System.Text.Json;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace SoundChangerBlazorServer.Data
{
    public class YoutubeDownloader
    {
        private readonly YoutubeClient youtubeClient;
        private readonly YouTubeService YouTubeService;

        private readonly string BaseUrl = "https://www.youtube.com/";
        private readonly string BaseVideoUrl = "https://www.youtube.com/watch?v=";
        private readonly string BasePlaylistUrl = "https://www.youtube.com/playlist?list=";

        public YoutubeDownloader(IOptions<YoutubeApiSettings> youtubeOptions)
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
            if (videoInfo.Duration > TimeSpan.FromMinutes(10))
            {
                return ("", "");
            }

            var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoUrl);
            var streamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();

            var restrictedCharacters = new char[] { '\\', '/', ':', '*', '?', '\"', '<', '>', '|', '+', '#' };
            var videoTitle = restrictedCharacters.Aggregate(videoInfo.Title, (c1, c2) => c1.Replace(c2, ' '));

            var filePath = Path.Combine(path, videoTitle + ".mp4");
            await youtubeClient.Videos.Streams.DownloadAsync(streamInfo, filePath);

            return (videoTitle, filePath);
        }

        public async Task<IEnumerable<YoutubeVideo>> GetVideosAsync(string query)
        {
            var videos = await youtubeClient.Search.GetVideosAsync(query);

            return videos.Select(x => new YoutubeVideo()
            {
                Title = x.Title,
                Url = x.Url,
                ImgUrl = x.Thumbnails[3].Url,
                Id = x.Id
            });
        }

        public async Task<YoutubeVideo> GetVideoAsync(string query)
        {
            var youtubeVideo = new YoutubeVideo();
            if (query.Contains(BaseVideoUrl))
            {
                var video = await youtubeClient.Videos.GetAsync(query);
                youtubeVideo.Id = video.Id;
                youtubeVideo.Url = video.Url;
                youtubeVideo.Title = video.Title;
                youtubeVideo.ImgUrl = video.Thumbnails[3].Url;
            }
            else
            {
                var videoSearch = (await youtubeClient.Search.GetVideosAsync(query)).First();
                youtubeVideo.Id = videoSearch.Id;
                youtubeVideo.Url = videoSearch.Url;
                youtubeVideo.Title = videoSearch.Title;
                youtubeVideo.ImgUrl = videoSearch.Thumbnails[3].Url;
            }

            return youtubeVideo;
        }

        public async Task<string> GetVideoIdByTitleAsync(string titleAuthor)
        {
            var searchListRequest = YouTubeService.Search.List("snippet");
            searchListRequest.Q = titleAuthor;
            searchListRequest.MaxResults = 5;

            var searchListResponse = await searchListRequest.ExecuteAsync();

            return searchListResponse.Items[0].Id.VideoId;
        }

        public async Task<YoutubePlaylist> GetPlaylistAsync(string query)
        {
            var youtubePlaylist = new YoutubePlaylist();

            if (query.Contains(BasePlaylistUrl))
            {
                var playlist = await youtubeClient.Playlists.GetAsync(query);
                youtubePlaylist.Id = playlist.Id;
                youtubePlaylist.Url = playlist.Url;
                youtubePlaylist.Title = playlist.Title;
                youtubePlaylist.ImgUrl = playlist.Thumbnails[3].Url;
            }
            else
            {
                var playlistsSearch = (await youtubeClient.Search.GetPlaylistsAsync(query)).First();
                youtubePlaylist.Id = playlistsSearch.Id;
                youtubePlaylist.Url = playlistsSearch.Url;
                youtubePlaylist.Title = playlistsSearch.Title;
                youtubePlaylist.ImgUrl = playlistsSearch.Thumbnails[3].Url;
            }

            var videos = await youtubeClient.Playlists.GetVideosAsync(youtubePlaylist.Id);

            youtubePlaylist.Videos = videos.Select(video => new YoutubeVideo
            {
                Id = video.Id,
                Title = video.Title,
                ImgUrl = video.Thumbnails[3].Url,
                Url = video.Url
            });

            return youtubePlaylist;
        }
    }
}
