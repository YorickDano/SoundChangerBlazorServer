using Microsoft.AspNetCore.Components.Forms;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundChangerBlazorServer.Models;
using SoundChangerBlazorServer.Services.Enums;
using SoundChangerBlazorServer.Services.Interfaces;
using SoundTouch.Net.NAudioSupport;
using System.Diagnostics;

namespace SoundChangerBlazorServer.Services
{
    public class AudioService : IAudioService
    {
        private List<AudioFile> _audioFiles = new List<AudioFile>();
        private List<AudioFile> _testAudioFiles = new List<AudioFile>();
        private AudioFile _audioFile = new AudioFile();
        private IWebHostEnvironment _hostingEnvironment;
        private IYoutubeDownloader YoutubeDownloader;
        private static object _lock = new object();

        public AudioService(IWebHostEnvironment hostingEnvironment, IYoutubeDownloader youtubeDownloader)
        {
            _hostingEnvironment = hostingEnvironment;
            YoutubeDownloader = youtubeDownloader;
        }

        public bool Any() => _audioFile != null && (_audioFiles?.Any() ?? false);
        public async Task<AudioFile> GetCurrentFile() => _audioFile;
        public async Task<IEnumerable<AudioFile>> GetList() => _audioFiles;
        public async Task<IEnumerable<AudioFile>> GetTestList() => _testAudioFiles;
        public async Task<AudioFile> FindFile(string title) => _audioFiles.First(x => x.Title == title);
        public async Task ReturnToOrigin() => _audioFile = _audioFiles[0];
        public async Task ReturnTo(int id) => _audioFile = _audioFiles.First(x => x.Id == id);
        public async Task UpdateImageUrl(string imgUrl) => _audioFile.ImageUrl = imgUrl;
        public async Task UpdateLyrics(string lyrics) => _audioFile.Lyrics = lyrics;
        public async Task Delete(int id)
        {
            var audioToRemove = _audioFiles.FirstOrDefault(x => x.Id == id) ?? throw new Exception("Incorrect id!");
            _audioFiles.Remove(audioToRemove);
            File.Delete(audioToRemove.FilePath);
            _audioFile = _audioFiles.LastOrDefault(new AudioFile());
        }
        public async Task<double> GetAllSize(SizeType type = SizeType.Megabyte) => type switch
        {
            SizeType.Byte => _audioFiles.Sum(a => a.Size),
            SizeType.Kilobyte => _audioFiles.Sum(a => a.Size / 1024.0),
            SizeType.Megabyte => _audioFiles.Sum(a => a.Size / 1024.0 / 1024),
            SizeType.Gigabyte => _audioFiles.Sum(a => a.Size / 1024.0 / 1024 / 1024),
            _ => throw new Exception($"Cannot hnadle this type {type}")
        };
        public async Task UpdateCurrentToLast()
        {
            if (Any())
            {
                _audioFile = _audioFiles.Last();
            }
        }
        public async Task AddAudio(AudioFile audio)
        {
            audio.Id = _audioFiles.Count + 1;
            audio.WWWRootPath = _hostingEnvironment.WebRootPath;
            _audioFiles.Add(audio);
            _testAudioFiles.Add(audio);
        }

        public async Task DeleteAllAsync()
        {
            await Task.Run(() =>
            {
                foreach (var file in _audioFiles.Select(x => x.FilePath))
                {
                    File.Delete(file);
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

        public async Task Update()
        {
            var directoryInfo = new DirectoryInfo(_hostingEnvironment.WebRootPath);
            _audioFiles.AddRange(directoryInfo.GetFiles("*.wav")
                                              .Select(f => AudioFile.Init(f.FullName)));
            _audioFile = _audioFiles.LastOrDefault(new AudioFile());
        }

        public async Task<bool> FileInput(InputFileChangeEventArgs e)
        {
            _audioFile = AudioFile.Init(e);
            _audioFile.Id = _audioFiles.Count + 1;
            _audioFile.WWWRootPath = _hostingEnvironment.WebRootPath;
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

        public async Task<bool> LoadAudio(string videoId)
        {
            var fileInfo = await YoutubeDownloader.Download(videoId);

            if (string.IsNullOrEmpty(fileInfo.Path))
            {
                return false;
            }

            var audioFile = AudioFile.Init(fileInfo.Path);
            audioFile.Id = _audioFiles.Count + 1;
            audioFile.VideoId = videoId;
            audioFile.Extension = ".wav";
            audioFile.WWWRootPath = _hostingEnvironment.WebRootPath;
            audioFile.Author = fileInfo.Video.ChannelTitle.Replace("- Topic", "")
                                                          .Trim();
            audioFile.Title = fileInfo.Video.Title;
            audioFile.Size = new FileInfo(audioFile.FilePath).Length;

            audioFile.Created = true;
            _audioFiles.Add(audioFile);
            _audioFile = _audioFiles[^1];

            return true;
        }

        public async Task ChangeSound(SoundTouchSettings settings)
        {
            var newAudioFile = new AudioFile();
            _audioFile.WWWRootPath = _hostingEnvironment.WebRootPath;
            _audioFile.CopyTo(newAudioFile);
            newAudioFile.FileName += $" T-{settings.Tempo}_P-{settings.Pitch}_R-{settings.Rate}_V-{settings.Volume}_Q-{settings.Quality}";

            await Change(_audioFile.FilePath, newAudioFile.FilePath, settings);

            newAudioFile.Created = true;
            newAudioFile.Tempo = settings.Tempo;
            newAudioFile.Pitch = settings.Pitch;
            newAudioFile.Rate = settings.Rate;
            newAudioFile.Size = new FileInfo(newAudioFile.FilePath).Length;
            newAudioFile.Id = _audioFiles.Count + 1;
            newAudioFile.IsChanged = true;
            _audioFiles.Add(newAudioFile);
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
                        var r = 0.7 + (rand.NextDouble() * (1.3 - 0.7));
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

        public async Task<AudioFile> ConvertCurrentToMp3()
        {
            var mp3File = new AudioFile();
            _audioFile.CopyTo(mp3File);
            var inputWavPath = _audioFile.FilePath;
            mp3File.Extension = ".mp3";
            var outputMp3Path = mp3File.FilePath;
            string arguments = $"-i \"{inputWavPath}\" -vn -ar 44100 -ac 2 -b:a 320k \"{outputMp3Path}\"";
            lock (_lock)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(_hostingEnvironment.WebRootPath, "ffmpeg.exe"),
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    process.WaitForExit();
                }
            }

            return mp3File;
        }

        private async Task Change(string fromPath, string toPath, SoundTouchSettings settings)
        {
            using var audioFile = new AudioFileReader(fromPath);
            audioFile.Volume = (float)settings.Volume;

            var waveFormat = new WaveFormat((int)(audioFile.WaveFormat.SampleRate * settings.Quality), audioFile.WaveFormat.BitsPerSample, audioFile.WaveFormat.Channels);

            var soundTouchProvider = new SoundTouchWaveProvider(audioFile)
            {
                Tempo = settings.Tempo,
                Pitch = settings.Pitch,
                Rate = settings.Rate
            };

            await Task.Run(async () =>
            {
                using var resampler = new MediaFoundationResampler(soundTouchProvider, waveFormat);

                await using var fileStream = new FileStream(
                    toPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true);
                WaveFileWriter.WriteWavFileToStream(fileStream, resampler);
            });
        }

        public async Task TestChange()
        {
            var rand = new Random();

            foreach (var file in _testAudioFiles.Where(x => !x.IsChanged))
            {
                var randVal = 0.7 + (rand.NextDouble() * (1.3 - 0.7));
                var settings = new SoundTouchSettings
                {
                    Rate = randVal,
                    Pitch = randVal > 1 ? randVal + 0.02 : randVal - 0.02,
                };
                var tmpFileName = Path.Combine(file.WWWRootPath, file.FileName + 't' + file.Extension);
                await Change(file.FilePath, tmpFileName, settings);
                File.Delete(file.FilePath);
                File.Move(tmpFileName, file.FilePath);
                file.IsChanged = true;
            }
        }
    }
}
