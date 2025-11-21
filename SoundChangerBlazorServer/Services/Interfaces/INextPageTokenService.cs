namespace SoundChangerBlazorServer.Services.Interfaces
{
    public interface INextPageTokenService
    {
        string? NextPageToken { get; set; }
        string? FirstNextPageToken { get; set; }
    }
}
