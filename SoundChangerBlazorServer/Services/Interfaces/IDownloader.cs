using SoundChangerBlazorServer.Models.YoutubeModels;

namespace SoundChangerBlazorServer.Services.Interfaces
{
    public interface IDownloader
    {
        Task<(YoutubeVideo Video, string Path)> Download(string videoId);
    }
}
