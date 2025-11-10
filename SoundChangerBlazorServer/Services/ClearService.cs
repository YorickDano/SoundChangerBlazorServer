using SoundChangerBlazorServer.Services.Interfaces;

namespace SoundChangerBlazorServer.Services
{
    public class ClearService : IClearService
    {
        private readonly IWebHostEnvironment _environment;

        public ClearService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task ClearFiles()
        {
            var directoryInfo = new DirectoryInfo(_environment.WebRootPath);
            await Task.Run(() =>
            {
                foreach (var file in directoryInfo.GetFiles("*.wav"))
                {
                    file.Delete();
                }
            });
        }
    }
}
