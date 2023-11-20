using MediaToolkit;
using MediaToolkit.Model;
using Microsoft.AspNetCore.Components.Forms;
using NAudio.MediaFoundation;
using NAudio.Wave;
using SoundChangerBlazorServer.Models;
using SoundTouch.Net.NAudioSupport;

namespace SoundChangerBlazorServer.Data
{
    public class AudioService
    {
        private List<AudioFile> _audioFiles = new List<AudioFile>();
        private AudioFile _audioFile = new AudioFile();
        private IWebHostEnvironment _hostingEnvironment;
        private YoutubeDownloader YoutubeDownloader;

        public AudioService(IWebHostEnvironment hostingEnvironment, YoutubeDownloader youtubeDownloader)
        {
            _hostingEnvironment = hostingEnvironment;
            YoutubeDownloader = youtubeDownloader;
        }

        public async Task<AudioFile> GetCurrentFile() => _audioFile;
        public async Task<IEnumerable<AudioFile>> GetList() => _audioFiles;
        public async Task<AudioFile> FindFile(string title) => _audioFiles.First(x => x.Title == title);
        public async Task ReturnToOrigin() => _audioFile = _audioFiles[0];
        public async Task ReturnTo(int id) => _audioFile = _audioFiles.First(x => x.Id == id);

        public async Task DeleteAllAsync()
        {
            var directoryInfo = new DirectoryInfo(_hostingEnvironment.WebRootPath);
            await Task.Run(() =>
            {
                foreach (var file in directoryInfo.GetFiles("*.wav").Where(x=> x.Name != _audioFile.FileName))
                {
                    file.Delete();
                }
            });
            _audioFiles.Clear();
            _audioFiles.Add(_audioFile);
        }

        public async Task Clear()
        {
            var directoryInfo = new DirectoryInfo(_hostingEnvironment.WebRootPath);
            await Task.Run(() =>
            {
                foreach (var file in directoryInfo.GetFiles("*.wav"))
                {
                    file.Delete();
                }
            });
        }

        public async Task ReturnToPrevious()
        {
            var index = _audioFiles.IndexOf(_audioFile);

            if (index != 0)
            {
                _audioFile = _audioFiles[index - 1];
            }
        }

        public async Task FileInput(InputFileChangeEventArgs e)
        {
            var wwwrootPath = _hostingEnvironment.WebRootPath;
            _audioFile = new AudioFile()
            {
                Id = _audioFiles.Count,
                FileName = Path.GetFileNameWithoutExtension(e.File.Name),
                Title = Path.GetFileNameWithoutExtension(e.File.Name),
                Extension = Path.GetExtension(e.File.Name),
                Format = e.File.ContentType,
                WWWRootPath = wwwrootPath,
            };
            _audioFile.FileName += _audioFiles.Count;

            if (_audioFile.Extension == ".mp3")
            {
                var oldPath = _audioFile.FilePath;
                using (var fs = File.Create(_audioFile.FilePath))
                {
                    await e.File.OpenReadStream(maxAllowedSize: 1024 * 1024 * 64).CopyToAsync(fs);
                }

                using (var mp3 = new Mp3FileReader(_audioFile.FilePath))
                {
                    _audioFile.Extension = ".wav";
                    _audioFile.Duration = mp3.TotalTime;
                    using (var pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
                    {
                        WaveFileWriter.CreateWaveFile(_audioFile.FilePath, pcm);
                    }
                }
                File.Delete(oldPath);
                _audioFile.Created = true;
                _audioFiles.Add(_audioFile);
                return;
            }

            using var fileStream = File.Create(_audioFile.FilePath);

            await e.File.OpenReadStream(maxAllowedSize: 1024 * 1024 * 64).CopyToAsync(fileStream);
            _audioFile.Created = true;
            _audioFiles.Add(_audioFile);

        }

        public async Task<bool> LoadFromYoutube(string youtubeLink)
        {
            var wwwrootPath = Path.Combine(_hostingEnvironment.WebRootPath);

            var fileInfo = await YoutubeDownloader.Download(youtubeLink, wwwrootPath);

            if (string.IsNullOrEmpty(fileInfo.Item1))
            {
                return false;
            }

            _audioFile = new AudioFile()
            {
                Id = _audioFiles.Count,
                FileName = Path.GetFileNameWithoutExtension(fileInfo.Item1),
                Title = Path.GetFileNameWithoutExtension(fileInfo.Item1),
                Extension = ".wav",
                Format = "WAVE",
                WWWRootPath = wwwrootPath,
            };
            _audioFile.FileName += _audioFiles.Count;
            var fileCreation = File.Create(_audioFile.FilePath);
            fileCreation.Close();
           
            using (var video = new MediaFoundationReader(fileInfo.Item2))
            {
                WaveFileWriter.CreateWaveFile(_audioFile.FilePath, video);
            }

            _audioFile.Created = true;
            _audioFiles.Add(_audioFile);

            File.Delete(fileInfo.Item2);

            return true;
        }

        public async Task ChangeSound(SoundTouchSettings settings)
        {
            var newAudioFile = new AudioFile();
            _audioFile.CopyTo(newAudioFile);
            newAudioFile.FileName = newAudioFile.FileName.Remove(newAudioFile.FileName.Length - 1) + _audioFiles.Count;
            using var audioFile = new AudioFileReader(_audioFile.FilePath);

            var soundTouchProvider = new SoundTouchWaveProvider(audioFile)
            {
                Tempo = settings.Tempo,
                Pitch = settings.Pitch,
                Rate = settings.Rate,
            };


            await Task.Run(() =>
            {
                WaveFileWriter.CreateWaveFile(newAudioFile.FilePath, soundTouchProvider);
            });

            audioFile.Close();
            newAudioFile.Created = true;
            newAudioFile.Tempo = settings.Tempo;
            newAudioFile.Pitch = settings.Pitch;
            newAudioFile.Rate = settings.Rate;
            _audioFiles.Add(newAudioFile);
            _audioFile.Id = _audioFiles.Count;
            _audioFile = _audioFiles[^1];
        }

        public async Task ConvertCurrentToMp3()
        {
            using var fileReader = new WaveFileReader(_audioFile.FilePath);
            _audioFile.Extension = ".mp3";
            var mediaType = MediaFoundationEncoder.SelectMediaType(AudioSubtypes.MFAudioFormat_MP3,
                            fileReader.WaveFormat, 192000);
            using (var encoder = new MediaFoundationEncoder(mediaType))
            {
                MediaFoundationApi.Startup();
                MediaFoundationEncoder.EncodeToMp3(fileReader, _audioFile.FilePath, 192000);
                MediaFoundationApi.Shutdown();
            }
        }



        public async Task<AudioFile> CutFromTo(AudioFile audioFile, TimeSpan from, TimeSpan to)
        {
            using var fileReader = new AudioFileReader(audioFile.FilePath);


            return null;
        }
    }
}
