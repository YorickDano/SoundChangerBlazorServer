using Microsoft.AspNetCore.Components.Forms;
using SoundChangerBlazorServer.Models;

namespace SoundChangerBlazorServer.Services.Interfaces
{
    public interface IAudioService
    {
        Task<bool> LoadAudio(string source);
        Task<bool> FileInput(InputFileChangeEventArgs e);
        Task ChangeSound(SoundTouchSettings settings);
        Task Mix(int seconds);
        bool Any();
        Task<AudioFile> GetCurrentFile();
        Task<IEnumerable<AudioFile>> GetList();
        Task<AudioFile> FindFile(string title);
        Task ReturnToOrigin();
        Task ReturnTo(int id);
        Task DeleteAllAsync();
        Task ReturnToPrevious();
        Task UpdateImageUrl(string imgUrl);
        Task UpdateLyrics(string lyrics);
    }
}
