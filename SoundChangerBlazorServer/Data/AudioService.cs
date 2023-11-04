using MediaToolkit;
using MediaToolkit.Model;
using Microsoft.AspNetCore.Components.Forms;
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
        public async Task ReturnToOrigin() => _audioFile = _audioFiles[0];
        public async Task ReturnTo(int id) => _audioFile = _audioFiles.First(x => x.Id == id);

        public async Task DeleteAll()
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

        public async Task LoadFromYoutube(string youtubeLink)
        {
            var wwwrootPath = Path.Combine(_hostingEnvironment.WebRootPath);

            var fileNameAndPath = await YoutubeDownloader.Download(youtubeLink, wwwrootPath);
            _audioFile = new AudioFile()
            {
                Id = _audioFiles.Count,
                FileName = Path.GetFileNameWithoutExtension(fileNameAndPath.Item1),
                Title = Path.GetFileNameWithoutExtension(fileNameAndPath.Item1),
                Extension = ".wav",
                Format = "WAVE",
                WWWRootPath = wwwrootPath,
            };
            _audioFile.FileName += _audioFiles.Count;

            var fileCreation = File.Create(_audioFile.FilePath);
            fileCreation.Close();
            var inputFile = new MediaFile { Filename = fileNameAndPath.Item2 };
            var outputFile = new MediaFile { Filename = _audioFile.FilePath };

            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);

                engine.Convert(inputFile, outputFile);
            }

            _audioFile.Created = true;
    
            _audioFiles.Add(_audioFile);
            File.Delete(fileNameAndPath.Item2);
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

            var fs = File.Create(newAudioFile.FilePath);
            fs.Close();

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
    }
}
