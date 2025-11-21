using SoundChangerBlazorServer.Services.Interfaces;

namespace SoundChangerBlazorServer.Services.YoutubeServices
{
    public class YoutubeNextPageTokenService : INextPageTokenService
    {
        public string? NextPageToken { get; set; }

        public string? FirstNextPageToken { get; set; }

        public YoutubeNextPageTokenService() { }
    }
}
