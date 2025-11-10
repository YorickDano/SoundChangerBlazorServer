namespace SoundChangerBlazorServer.Models.YoutubeModels
{
    public class YoutubeDownloaderSettings
    {
        public required string BaseUrl { get; set; }
        public required string BaseVideoUrl { get; set; }
        public required string BasePlaylistUrl { get; set; }
    }
}
