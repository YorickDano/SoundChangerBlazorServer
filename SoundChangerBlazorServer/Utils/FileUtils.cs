namespace SoundChangerBlazorServer.Utils
{
    public static class FileUtils
    {
        public static async Task<IList<string>> GetAllFilesContains(string path, string common)
        {
            ArgumentNullException.ThrowIfNull(path);
            ArgumentNullException.ThrowIfNull(common);

            if (!Directory.Exists(path))
            {
                throw new ArgumentException(nameof(path));
            }

            return [.. Directory.EnumerateFiles(path)
                                .Where(x => x.Contains(common, StringComparison.OrdinalIgnoreCase))];
        }
    }
}
