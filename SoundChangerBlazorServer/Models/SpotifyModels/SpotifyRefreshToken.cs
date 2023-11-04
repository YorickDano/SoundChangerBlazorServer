namespace SoundChangerBlazorServer.Models.SpotifyModels
{
    public class SpotifyRefreshToken
    {
        public string GrantType { get; set; }
        public string RefreshToken { get; set; }
        public string ClientId { get; set; }
    }
}
