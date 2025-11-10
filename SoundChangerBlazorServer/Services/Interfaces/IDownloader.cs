namespace SoundChangerBlazorServer.Services.Interfaces
{
    public interface IDownloader
    {
        Task<(string, string)> Download(string videoId);
    }
}
