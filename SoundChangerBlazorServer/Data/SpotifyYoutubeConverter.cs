namespace SoundChangerBlazorServer.Data
{
    public class SpotifyYoutubeConverter
    {
        public SpotifyClient Client { get; set; }
        private YoutubeDownloader YoutubeDownloader { get; set; }

        public SpotifyYoutubeConverter(SpotifyClient client, YoutubeDownloader youtubeDownloader)
        {
            Client = client;
            YoutubeDownloader = youtubeDownloader;
        }


        public async Task<bool> YoutubeToSpotifySong(string url)
        {
            var video = await YoutubeDownloader.GetVideoAsync(url);      

            return false;
        }
    }
}
