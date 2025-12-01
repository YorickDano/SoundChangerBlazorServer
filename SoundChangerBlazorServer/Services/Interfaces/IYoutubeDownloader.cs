using SoundChangerBlazorServer.Models.YoutubeModels;

namespace SoundChangerBlazorServer.Services.Interfaces
{
    public interface IYoutubeDownloader : IDownloader
    {
        Task<IEnumerable<YoutubeVideo>> GetVideosAsync(string query);
        Task<YoutubePlaylist> GetPlaylistAsync(string query);
    }
}
