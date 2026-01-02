using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using SoundChangerBlazorServer.Services.Interfaces;

namespace SoundChangerBlazorServer.Services
{
    public class TokenStorageService : ITokenStorageService
    {
        private readonly IDataProtector _protector;
        private readonly IMemoryCache _cache;
        private const string YoutubeTokenPrefix = "YouTubeTokens_";

        public TokenStorageService(IDataProtectionProvider provider,
                                   IMemoryCache cache)
        {
            _protector = provider.CreateProtector("TokenStorage");
            _cache = cache;
        }

        public Task StoreTokensAsync(string userId, string accessToken, string refreshToken, DateTime expiresAt)
        {
            var cacheKey = $"{YoutubeTokenPrefix}{userId}";

            var tokenData = new TokenData
            {
                AccessToken = accessToken,
                EncryptedRefreshToken = _protector.Protect(refreshToken),
                ExpiresAt = expiresAt,
                UpdatedAt = DateTime.UtcNow
            };

            var cachDuration = expiresAt - DateTime.UtcNow;
            if (cachDuration > TimeSpan.Zero)
            {
                _cache.Set(cacheKey, tokenData, cachDuration.Add(TimeSpan.FromMinutes(5)));
            }

            return Task.CompletedTask;
        }

        public Task<bool> HasTokensAsync(string userId)
        {
            var cacheKey = $"{YoutubeTokenPrefix}{userId}";
            return Task.FromResult(_cache.TryGetValue<TokenData>(cacheKey, out _));
        }

        public Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> GetTokensAsync(string userId)
        {
            var cacheKey = $"{YoutubeTokenPrefix}{userId}";

            if (!_cache.TryGetValue<TokenData>(cacheKey, out var tokenData))
            {
                return Task.FromResult<(string, string, DateTime)>((null, null, DateTime.MinValue));
            }

            try
            {
                var refreshToken = _protector.Unprotect(tokenData!.EncryptedRefreshToken);
                return Task.FromResult((tokenData.AccessToken, refreshToken, DateTime.MinValue));
            }
            catch (Exception ex) 
            {
                _cache.Remove(cacheKey);
                return Task.FromResult<(string, string, DateTime)>((null, null, DateTime.MinValue));
            }
        }

        public Task ClearTokensAsync(string userId)
        {
            var cacheKey = $"{YoutubeTokenPrefix}{userId}";
            _cache.Remove(cacheKey);
            return Task.CompletedTask;
        }


        private class TokenData
        {
            public string AccessToken { get; set; }
            public string EncryptedRefreshToken { get; set; }
            public DateTime ExpiresAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}
