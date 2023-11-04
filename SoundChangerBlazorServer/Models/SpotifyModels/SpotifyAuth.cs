namespace SoundChangerBlazorServer.Models.SpotifyModels
{
    public class SpotifyAuth
    {
        public string grant_type { get; set; }
        public string code { get; set; }
        public string redirect_uri { get; set; }
    }
}
