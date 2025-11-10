using Microsoft.AspNetCore.Components.Forms;

namespace SoundChangerBlazorServer.Models
{
    public class AudioFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Extension { get; set; }
        public string Format { get; set; }
        public bool Created { get; set; } = false;
        public string WWWRootPath { get; set; }
        public double Tempo { get; set; } = 1;
        public double Pitch { get; set; } = 1;
        public double Rate { get; set; } = 1;
        public TimeSpan Duration { get; set; }

        public string FilePath
        {
            get
            {
                return Path.Combine(WWWRootPath, FileName + Extension);
            }
            private set { }
        }
        public string AudioPath
        {
            get => FileName + Extension;

            private set { }
        }

        public void CopyTo(AudioFile anotherAf)
        {
            anotherAf.FileName = new string(this.FileName);
            anotherAf.Extension = new string(this.Extension);
            anotherAf.Format = new string(this.Format);
            anotherAf.WWWRootPath = new string(this.WWWRootPath);
            anotherAf.Title = new string(this.Title);
        }

        public static AudioFile Init(InputFileChangeEventArgs e) => 
            new AudioFile()
            {
                FileName = Path.GetFileNameWithoutExtension(e.File.Name),
                Title = Path.GetFileNameWithoutExtension(e.File.Name),
                Extension = Path.GetExtension(e.File.Name),
                Format = e.File.ContentType,
            };
        }
    
}
