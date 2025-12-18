using SoundChangerBlazorServer.Models;
using SoundChangerBlazorServer.Services.Interfaces;

namespace SoundChangerBlazorServer.Services
{
    public class CutterService
    {
        public readonly IAudioService _audioService;

        public CutterService(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public async Task<AudioFile> GetCurrent()
        {
            return await _audioService.GetCurrentFile();
        }
    }
}
