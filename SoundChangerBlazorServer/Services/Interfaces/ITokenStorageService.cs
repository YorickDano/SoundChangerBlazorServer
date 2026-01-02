namespace SoundChangerBlazorServer.Services.Interfaces
{
    public interface ITokenStorageService
    {
        Task StoreTokensAsync(string userId, string accessToken, string refreshToken, DateTime expiresAt);
        Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> GetTokensAsync(string userId);
        Task ClearTokensAsync(string userId);
        Task<bool> HasTokensAsync(string userId);
    }
}
