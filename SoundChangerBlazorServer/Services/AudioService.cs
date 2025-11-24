using Microsoft.AspNetCore.Components.Forms;
using NAudio.MediaFoundation;
using NAudio.Wave;
using SoundChangerBlazorServer.Models;
using SoundTouch.Net.NAudioSupport;
using NAudio.Wave.SampleProviders;
using SoundChangerBlazorServer.Services.Interfaces;

namespace SoundChangerBlazorServer.Services
{
    public class AudioService : IAudioService
    {
        private List<AudioFile> _audioFiles = new List<AudioFile>();
        private AudioFile _audioFile = new AudioFile();
        private IWebHostEnvironment _hostingEnvironment;
        private IYoutubeDownloader YoutubeDownloader;

        public AudioService(IWebHostEnvironment hostingEnvironment, IYoutubeDownloader youtubeDownloader)
        {
            _hostingEnvironment = hostingEnvironment;
            YoutubeDownloader = youtubeDownloader;
        }

        public bool Any() => _audioFile != null && (_audioFiles?.Any() ?? false);
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
                foreach (var file in directoryInfo.GetFiles("*.wav").Where(x => x.Name != _audioFile.FileName))
                {
                    file.Delete();
                }
            });
            _audioFiles.Clear();
            _audioFiles.Add(_audioFile);
        }

        public async Task ReturnToPrevious()
        {
            var index = _audioFiles.IndexOf(_audioFile);

            if (index != 0)
            {
                _audioFile = _audioFiles[index - 1];
            }
        }

        public async Task<bool> FileInput(InputFileChangeEventArgs e)
        {
            var wwwrootPath = _hostingEnvironment.WebRootPath;
            _audioFile = AudioFile.Init(e);
            _audioFile.Id = _audioFiles.Count;
            _audioFile.WWWRootPath = wwwrootPath;
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
                return true;
            }

            using var fileStream = File.Create(_audioFile.FilePath);

            await e.File.OpenReadStream(maxAllowedSize: 1024 * 1024 * 64).CopyToAsync(fileStream);
            _audioFile.Created = true;
            _audioFiles.Add(_audioFile);

            return true;
        }

        public async Task<bool> LoadAudio(string youtubeLink)
        {
            var fileInfo = await YoutubeDownloader.Download(youtubeLink);

            if (string.IsNullOrEmpty(fileInfo.Name))
            {
                return false;
            }

            var audioFile = new AudioFile()
            {
                Id = _audioFiles.Count,
                FileName = Path.GetFileNameWithoutExtension(fileInfo.Name),
                Title = Path.GetFileNameWithoutExtension(fileInfo.Name),
                Extension = ".wav",
                Format = "WAVE",
                WWWRootPath = _hostingEnvironment.WebRootPath,
            };
            audioFile.FileName += _audioFiles.Count;

            File.Move(fileInfo.Path, audioFile.FilePath);

            audioFile.Created = true;
            _audioFiles.Add(audioFile);
            _audioFile = _audioFiles[^1];
            File.Delete(fileInfo.Path);

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

        public async Task Mix(int seconds = 10)
        {
            seconds = seconds == 0 ? 40 : seconds * 4;
            var direc = Directory.CreateDirectory(Path.Combine(_audioFile.WWWRootPath, _audioFile.Title));
            direc.Create();
            var readySamplesBySeconds = new List<byte[]>();
            var testSamples = new List<byte[]>();
            WaveFormat? format = null;
            var buffSize = 0;

            using (var wave1 = new AudioFileReader(_audioFile.FilePath))
            {
                var samplesPerSecond = wave1.WaveFormat.SampleRate * wave1.WaveFormat.Channels;
                var testBuffer = new byte[samplesPerSecond];

                while (await wave1.ReadAsync(testBuffer) > 0)
                {
                    testSamples.Add([.. testBuffer]);
                }
            }

            using (var wave = new AudioFileReader(_audioFile.FilePath))
            {
                var samplesPerSecond = wave.WaveFormat.SampleRate * wave.WaveFormat.Channels;
                buffSize = samplesPerSecond * seconds;
                var readBuffer = new byte[buffSize];

                while (await wave.ReadAsync(readBuffer) > 0)
                {
                    readySamplesBySeconds.Add([.. readBuffer]);
                }

                var firsts = readySamplesBySeconds[0].Take(20).ToArray();
                var ind = 0;
                var lastInd = 0;
                for (var i = 0; i < readySamplesBySeconds[^1].Length; ++i)
                {
                    while (ind < 20 && readySamplesBySeconds[^1][i] == firsts[ind])
                    {
                        ++ind;
                    }

                    if (ind == 19)
                    {
                        lastInd = i;
                        break;
                    }
                    else
                    {
                        ind = 0;
                    }
                }

                readySamplesBySeconds[^1] = readySamplesBySeconds[^1].Take(lastInd + 1).ToArray();

                format = wave.WaveFormat;
            }


            var rand = new Random();
            for (int i = 0; i < readySamplesBySeconds.Count; ++i)
            {
                var filePath = Path.Combine(direc.FullName, i + ".wav");
                using (var writer = new WaveFileWriter(filePath, format))
                {
                    await writer.WriteAsync(readySamplesBySeconds[i], 0, readySamplesBySeconds[i].Length);
                }

                using (var fs = new FileStream(filePath, FileMode.Open,
                           FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    using (var reader = new WaveFileReader(fs))
                    {
                        var r = rand.NextSingle() * 2;
                        r = r > 1.7 ? r - 0.3f : r < 0.3 ? r + 0.3f : r;
                        var soundTouchProvider = new SoundTouchWaveProvider(reader)
                        {
                            Tempo = r,
                            Pitch = r < 1 ? r - 0.05 : r + 0.05
                        };

                        WaveFileWriter.CreateWaveFile(Path.Combine(direc.FullName, i + "Changed.wav"), soundTouchProvider);
                    }
                }
            }

            var newAudioFile = new AudioFile();
            _audioFile.CopyTo(newAudioFile);
            newAudioFile.FileName = newAudioFile.FileName.Remove(newAudioFile.FileName.Length - 1) + _audioFiles.Count;
            await Task.Run(() =>
            {
                var files = direc.GetFiles("*Changed.wav").OrderBy(x => x.Name);
                var readers = new List<AudioFileReader>();
                foreach (var file in files)
                {
                    readers.Add(new AudioFileReader(file.FullName));
                }
                var concatProvider = new ConcatenatingSampleProvider(readers);
                WaveFileWriter.CreateWaveFile16(newAudioFile.FilePath, concatProvider);
            });
            newAudioFile.Created = true;
            _audioFiles.Add(newAudioFile);
            _audioFile.Id = _audioFiles.Count;
            _audioFile = _audioFiles[^1];
        }

        public void ConvertCurrentToMp3()
        {
            using var fileReader = new WaveFileReader(_audioFile.FilePath);
            _audioFile.Extension = ".mp3";
            var mediaType = MediaFoundationEncoder.SelectMediaType(AudioSubtypes.MFAudioFormat_MP3,
                            fileReader.WaveFormat, 320000);
            using (var encoder = new MediaFoundationEncoder(mediaType))
            {
                MediaFoundationApi.Startup();
                MediaFoundationEncoder.EncodeToMp3(fileReader, _audioFile.FilePath, 320000);
                MediaFoundationApi.Shutdown();
            }
        }
    }
}
