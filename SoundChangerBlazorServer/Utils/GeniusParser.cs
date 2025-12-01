using HtmlAgilityPack;
using System.Net;
using System.Text.RegularExpressions;

namespace SoundChangerBlazorServer.Utils
{
    internal sealed class GeniusParser
    {
        private const string Xpath = "//div[contains(@class,'Lyrics__Container')]";

        public string Parse(string lyric)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(lyric);
            lyric = string.Join('\n', htmlDoc.DocumentNode.SelectNodes(Xpath).Select(x => x.InnerHtml));
            lyric = StripNewLines(lyric);
            lyric = StripTagsRegex(lyric);
            lyric = CleanEnding(lyric);
            lyric = WebUtility.HtmlDecode(lyric);
            lyric = lyric[(lyric.IndexOf(']') + 1)..];

            return lyric?.Trim() ?? string.Empty;
        }

        public static string StripTagsRegex(string source)
        {
            return Regex.Replace(source, "<[^>]*>", string.Empty);
        }

        public static string StripNewLines(string source)
        {
            return Regex.Replace(source, @"(<br>|<br />|<br/>|</ br>|</br>)", "\r\n");
        }

        public string Urlify(string source)
        {
            return Regex.Replace(source, " ", "%20");
        }

        public static string CleanEnding(string source)
        {
            char[] charsToTrim = { '<', 'b', 'r', '>', ' ', '/' };
            for (int i = 0; i < 20; i++)
            {
                source = source.TrimEnd(charsToTrim);
            }
            return source;
        }
    }
}
