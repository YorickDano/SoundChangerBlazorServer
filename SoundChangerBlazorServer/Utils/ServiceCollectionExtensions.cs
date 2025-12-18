using SoundChangerBlazorServer.Models.YoutubeModels;
using YoutubeExplode;

namespace SoundChangerBlazorServer.Utils
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddYoutubeClient(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection(nameof(YoutubeClientSettings)).Get<YoutubeClientSettings>();
            if (string.IsNullOrEmpty(settings?.BaseUrl)) 
                return services;
            services.AddHttpClient(nameof(YoutubeClient), client =>
            {
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            });

            return services;
        }
    }
}
