namespace SoundChangerBlazorServer.Utils
{
    public static class Extensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            var elements = source.ToArray();
            var random = Random.Shared;

            for (int i = elements.Length - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                (elements[i], elements[swapIndex]) = (elements[swapIndex], elements[i]);
            }

            return elements;
        }
    }
}
