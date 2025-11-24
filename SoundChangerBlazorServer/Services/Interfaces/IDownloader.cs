namespace SoundChangerBlazorServer.Services.Interfaces
{
    public interface IDownloader
    {
        Task<(string Name, string Path)> Download(string videoId);
    }
}
