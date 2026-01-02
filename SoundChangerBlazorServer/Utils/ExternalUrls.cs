namespace SoundChangerBlazorServer.Utils
{
    public static class ExternalUrls
    {
        public const string YouTube = "https://www.youtube.com/watch?v={0}";

        public static string YouTubeVideo(string id) =>
            string.Format(YouTube, id);
    }
}
