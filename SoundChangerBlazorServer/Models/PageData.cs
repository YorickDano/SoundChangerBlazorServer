using SoundChangerBlazorServer.Models.YoutubeModels;

namespace SoundChangerBlazorServer.Models
{
    public class PageData
    {
        public IEnumerable<AudioFile>? _audioFiles;
        public AudioFile _audioFile = new AudioFile();
        public IEnumerable<Track>? Tracks;
        public IEnumerable<YoutubeVideo>? Videos;
        public SoundTouchSettings _settings = new SoundTouchSettings();
        public bool Loading = false;
        public bool ViewPreviousFiles = false;
        public bool IsAlertMessage = false;
        public bool TracksHide = true;
        public bool VideosHide = true;
        public string? AlertMessage;
        public string? YoutubeSearch;
        public string YoutubeSearchType = "video";
        public string? SpotifySearch;
        public string SpotifySearchType = "track";
        public int MixSeconds;
    }
}
